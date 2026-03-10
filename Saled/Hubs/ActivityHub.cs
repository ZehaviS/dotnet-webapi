using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Hubs
{
    [Authorize]
    public class ActivityHub : Hub
    {
        // Map userId to connectionIds
        private static readonly Dictionary<int, HashSet<string>> userConnections = new();

        public override async Task OnConnectedAsync()
        {
            // Try several claim keys to support different token formats / casing.
            var userIdClaim = Context.User?.FindFirst("Id")
                              ?? Context.User?.FindFirst("id")
                              ?? Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            var usernameClaim = Context.User?.FindFirst("username") ?? Context.User?.FindFirst(System.Security.Claims.ClaimTypes.Name);

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                lock (userConnections)
                {
                    if (!userConnections.ContainsKey(userId))
                        userConnections[userId] = new HashSet<string>();
                    userConnections[userId].Add(Context.ConnectionId);
                }
                var msg = $"SignalR: userId={userId}, username={usernameClaim?.Value}, connectionId={Context.ConnectionId} connected";
                System.Diagnostics.Debug.WriteLine(msg);
                await Clients.Client(Context.ConnectionId).SendAsync("DebugLog", msg);
            }
            else
            {
                var msg = $"SignalR: userId not found in JWT claims! connectionId={Context.ConnectionId}, claims=[{string.Join(",", Context.User?.Claims.Select(c => c.Type + ":" + c.Value) ?? Array.Empty<string>())}]";
                System.Diagnostics.Debug.WriteLine(msg);
                await Clients.Client(Context.ConnectionId).SendAsync("DebugLog", msg);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userIdClaim = Context.User?.FindFirst("Id");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                lock (userConnections)
                {
                    if (userConnections.ContainsKey(userId))
                    {
                        userConnections[userId].Remove(Context.ConnectionId);
                        if (userConnections[userId].Count == 0)
                            userConnections.Remove(userId);
                    }
                }
            }
            await base.OnDisconnectedAsync(exception);
        }

        // Helper for SaledServiceJson to get all connectionIds for a user
        public static IReadOnlyCollection<string> GetConnectionsForUser(int userId)
        {
            lock (userConnections)
            {
                if (userConnections.TryGetValue(userId, out var set))
                    return set.ToList();
                return Array.Empty<string>();
            }
        }
    }
}
