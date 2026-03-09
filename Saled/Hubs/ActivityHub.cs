using Microsoft.AspNetCore.SignalR;

namespace Hubs
{
    public class ActivityHub : Hub
    {
        // Map userId to connectionIds
        private static readonly Dictionary<int, HashSet<string>> userConnections = new();

        public override async Task OnConnectedAsync()
        {
            var userIdClaim = Context.User?.FindFirst("Id");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                lock (userConnections)
                {
                    if (!userConnections.ContainsKey(userId))
                        userConnections[userId] = new HashSet<string>();
                    userConnections[userId].Add(Context.ConnectionId);
                }
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
