using Microsoft.AspNetCore.SignalR;
using Models;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;

namespace Services
{
    public class SaledServiceJson : ISaledsService
    {
        private readonly int activeUserId;
        private readonly bool isAdmin;
        private List<Saleds> Saleds { get; }
        private readonly Microsoft.AspNetCore.SignalR.IHubContext<Hubs.ActivityHub> hubContext;
        //private IWebHostEnvironment  webHost;
        private string filePath;
        public SaledServiceJson(
            IWebHostEnvironment webHost,
            IActiveUser activeUser,
            Microsoft.AspNetCore.SignalR.IHubContext<Hubs.ActivityHub> hubContext)
        {
            this.hubContext = hubContext;
            activeUserId = activeUser.ActiveUser?.Id ?? 0;
            isAdmin = activeUserId == 1; // אילה הוא מנהל ומקבל גישה לכל הסלטים

            this.filePath = Path.Combine(webHost.ContentRootPath, "Data", "Saled.json");
            using (var jsonFile = File.OpenText(filePath))
            {
                var content = jsonFile.ReadToEnd();
                Saleds = JsonSerializer.Deserialize<List<Saleds>>(content,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<Saleds>();
            }
        }

        private void saveToFile()
        {
            var text = JsonSerializer.Serialize(Saleds);
            File.WriteAllText(filePath, text);
        }
        public List<Saleds> GetAll() =>
            isAdmin
                ? Saleds.ToList()
                : Saleds.Where(p => p.UserId == activeUserId).ToList();

        public Saleds Get(int id)
        {
            var saled = Saleds.FirstOrDefault(p => p.Id == id);
            return isAdmin || saled?.UserId == activeUserId ? saled : null;
        }

        public void Add(Saleds saled)
        {
            saled.Id = Saleds.Count() + 1;
            saled.UserId = activeUserId;
            Saleds.Add(saled);
            saveToFile();
            BroadcastActivity($"{GetUserName()} 🥗 הוסיף/ה סלט חדש: '{saled.Name}'! איזה טעים! 😋");
        }

        public void Delete(int id)
        {
            var saled = Get(id);
            if (saled is null)
                return;

            // Admin can delete any salad; others only their own
            if (!isAdmin && saled.UserId != activeUserId)
                return;

            Saleds.Remove(saled);
            saveToFile();
            BroadcastActivity($"{GetUserName()} 🗑️ מחק/ה סלט: '{saled.Name}'. ביי ביי לסלט! 😢");
        }

        public void Update(Saleds saled)
        {
            var existing = Saleds.FirstOrDefault(p => p.Id == saled.Id);
            if (existing is null)
            {
                Console.WriteLine($"SaledServiceJson.Update: salad id={saled.Id} not found");
                return;
            }

            // If the caller didn't send a UserId (common in client updates), keep the existing one.
            if (saled.UserId == 0)
                saled.UserId = existing.UserId;

            // Admin can update any salad; others only their own
            if (!isAdmin && saled.UserId != activeUserId)
            {
                Console.WriteLine($"SaledServiceJson.Update: forbidden update for user {activeUserId} on salad {saled.Id} (owner {saled.UserId})");
                return;
            }

            if (!isAdmin && existing.UserId != activeUserId)
            {
                Console.WriteLine($"SaledServiceJson.Update: forbidden update because existing owner {existing.UserId} != current {activeUserId}");
                return;
            }

            Console.WriteLine($"SaledServiceJson.Update: before name={existing.Name} weight={existing.weight} image={existing.ImageUrl}");
            Console.WriteLine($"SaledServiceJson.Update: after  name={saled.Name} weight={saled.weight} image={saled.ImageUrl}");

            var index = Saleds.FindIndex(p => p.Id == saled.Id);
            if (index == -1)
            {
                Console.WriteLine($"SaledServiceJson.Update: could not find index for id={saled.Id}");
                return;
            }

            // שמור את התמונה הקיימת אם לא נבחרה חדשה
            if (string.IsNullOrEmpty(saled.ImageUrl))
                saled.ImageUrl = existing.ImageUrl;

            Saleds[index] = saled;
            saveToFile();
            BroadcastActivity($"{GetUserName()} ✏️ עדכן/ה סלט: '{saled.Name}'. שדרוג טעים! 🥒🥕");
        }

        private async void BroadcastActivity(string message)
        {
            var connections = Hubs.ActivityHub.GetConnectionsForUser(activeUserId);
            var connList = connections.Any() ? string.Join(",", connections) : "<none>";
            var debugMsg = $"BroadcastActivity: userId={activeUserId}, connections={connList}, message={message}";
            System.Diagnostics.Debug.WriteLine(debugMsg);
            Console.WriteLine(debugMsg);

            if (connections.Any())
            {
                await hubContext.Clients.Clients(connections).SendAsync("ReceiveActivity", message);
                await hubContext.Clients.Clients(connections).SendAsync("DebugLog", $"BroadcastActivity: Sent to {connections.Count} connections.");
            }
            else
            {
                // Fallback: if we don't have tracked connections (e.g., mapping failed)
                // broadcast to everyone so tabs still get updates.
                await hubContext.Clients.All.SendAsync("ReceiveActivity", message);
                await hubContext.Clients.All.SendAsync("DebugLog", $"BroadcastActivity: No connections found for userId={activeUserId}, broadcasting to all.");
            }
        }

        private string GetUserName()
        {
            // אילה היא מנהלת, כל השאר לפי ID
            if (activeUserId == 1) return "אילה";
            if (activeUserId == 2) return "זהבי";
            if (activeUserId == 3) return "מירי";
            if (activeUserId == 4) return "שירה";
            if (activeUserId == 5) return "מיכל";
            return $"משתמש {activeUserId}";
        }

        public int Count => GetAll().Count;
    }
}