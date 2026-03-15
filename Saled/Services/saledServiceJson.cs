using Microsoft.AspNetCore.SignalR;
using Models;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;

namespace Services
{
    public class SaledServiceJson : ISaledsService
    {
        private readonly int activeUserId;
        private readonly string activeUserName;
        private readonly bool isAdmin;
        private List<Saleds> Saleds { get; }
        private readonly IHubContext<Hubs.ActivityHub> hubContext;
        private string filePath;

        public SaledServiceJson(
            IWebHostEnvironment webHost,
            IActiveUser activeUser,
            IHubContext<Hubs.ActivityHub> hubContext)
        {
            this.hubContext = hubContext;
            activeUserId = activeUser.ActiveUser?.Id ?? 0;
            activeUserName = string.IsNullOrWhiteSpace(activeUser.ActiveUser?.Name)
                ? $"משתמש {activeUserId}"
                : activeUser.ActiveUser.Name;
            isAdmin = activeUser.ActiveUser?.ClearanceLevel == 1 || activeUserId == 1; // Clearance 1 או id=1 מנהל

            this.filePath = Path.Combine(webHost.ContentRootPath, "Data", "Saled.json");

            using (var jsonFile = File.OpenText(filePath))
            {
                var content = jsonFile.ReadToEnd();
                Saleds = JsonSerializer.Deserialize<List<Saleds>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                ) ?? new List<Saleds>();
            }
        }

        private void saveToFile()
        {
            var text = JsonSerializer.Serialize(Saleds);
            File.WriteAllText(filePath, text);
        }

        public List<Saleds> GetAll() =>
            isAdmin ? Saleds.ToList() : Saleds.Where(p => p.UserId == activeUserId).ToList();

        public Saleds Get(int id)
        {
            var saled = Saleds.FirstOrDefault(p => p.Id == id);
            return isAdmin || saled?.UserId == activeUserId ? saled : null;
        }

        public void Add(Saleds saled)
        {
            // יצירת ID ייחודי לפי המספר הכי גדול שקיים
            saled.Id = Saleds.Any() ? Saleds.Max(s => s.Id) + 1 : 1;

            saled.UserId = activeUserId;
            Saleds.Add(saled);
            saveToFile();
            BroadcastActivity($"{GetUserName()} 🥗 הוסיף/ה סלט חדש: '{saled.Name}'! איזה טעים! 😋");
        }

        public void Delete(int id)
        {
            var saled = Get(id);
            if (saled is null) return;

            if (!isAdmin && saled.UserId != activeUserId) return;

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

            if (saled.UserId == 0) saled.UserId = existing.UserId;

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

            var index = Saleds.FindIndex(p => p.Id == saled.Id);
            if (index == -1)
            {
                Console.WriteLine($"SaledServiceJson.Update: could not find index for id={saled.Id}");
                return;
            }

            if (string.IsNullOrEmpty(saled.ImageUrl)) saled.ImageUrl = existing.ImageUrl;

            Saleds[index] = saled;
            saveToFile();
            BroadcastActivity($"{GetUserName()} ✏️ עדכן/ה סלט: '{saled.Name}'. שדרוג טעים! 🥒🥕");
        }

        private async void BroadcastActivity(string message)
        {
            var connections = Hubs.ActivityHub.GetConnectionsForUser(activeUserId);
            var connList = connections.Any() ? string.Join(",", connections) : "<none>";
            var debugMsg = $"BroadcastActivity: userId={activeUserId}, connections={connList}, message={message}";
            Console.WriteLine(debugMsg);

            if (connections.Any())
            {
                await hubContext.Clients.Clients(connections).SendAsync("ReceiveActivity", message);
                await hubContext.Clients.Clients(connections).SendAsync("DebugLog", $"BroadcastActivity: Sent to {connections.Count} connections.");
            }
            else
            {
                await hubContext.Clients.All.SendAsync("ReceiveActivity", message);
                await hubContext.Clients.All.SendAsync("DebugLog", $"BroadcastActivity: No connections found for userId={activeUserId}, broadcasting to all.");
            }
        }

        private string GetUserName()
        {
            return activeUserName;
        }

        public List<Saleds> GetByUser(int userId)
        {
            return isAdmin ? Saleds.Where(p => p.UserId == userId).ToList() : Saleds.Where(p => p.UserId == activeUserId).ToList();
        }

        public int Count => GetAll().Count;
    }
}