using JewelryShop.DTO;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace JewelryShop.Hubs
{
    public class ChatHub : Hub
    {
        private static readonly ConcurrentDictionary<int, HashSet<string>> UserConnections
            = new ConcurrentDictionary<int, HashSet<string>>();

        // Lưu last active
        private static readonly ConcurrentDictionary<int, DateTime> LastActive
            = new ConcurrentDictionary<int, DateTime>();

        private static readonly ConcurrentDictionary<string, int> ConnectionToUser
            = new ConcurrentDictionary<string, int>();


        // Khi user kết nối SignalR
        public override async Task OnConnectedAsync()
        {
            var userIdStr = Context.GetHttpContext()?.Request.Query["userId"].ToString();
            if (!int.TryParse(userIdStr, out int userId))
            {
                await base.OnConnectedAsync();
                return;
            }

            var connectionId = Context.ConnectionId;
            ConnectionToUser[connectionId] = userId;
            Console.WriteLine($"Connected userId = {userId}");


            var connections = UserConnections.GetOrAdd(userId, _ => new HashSet<string>());
            bool shouldNotifyOnline = false;

            lock (connections)
            {
                connections.Add(connectionId);
                if (connections.Count == 1)
                    shouldNotifyOnline = true;
            }

            if (shouldNotifyOnline)
                await Clients.All.SendAsync("UserOnline", userId);

            await base.OnConnectedAsync();
        }


        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;

            if (!ConnectionToUser.TryRemove(connectionId, out int userId))
            {
                await base.OnDisconnectedAsync(exception);
                return;
            }

            bool shouldNotifyOffline = false;
            DateTime lastActiveTime = DateTime.UtcNow;

            if (UserConnections.TryGetValue(userId, out var connections))
            {
                lock (connections)
                {
                    connections.Remove(connectionId);
                    if (connections.Count == 0)
                    {
                        shouldNotifyOffline = true;
                        LastActive[userId] = lastActiveTime;
                    }
                }
            }

            if (shouldNotifyOffline)
            {
                await Clients.All.SendAsync("UserOffline", new
                {
                    userId,
                    lastActive = lastActiveTime.ToString("o")
                });
            }

            await base.OnDisconnectedAsync(exception);
        }


        public async Task JoinRoom(int chatRoomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, chatRoomId.ToString());
        }


        public async Task LeaveRoom(int chatRoomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatRoomId.ToString());
        }

        public async Task SendMessageToGroup(int chatRoomId, MessageDTO message)
        {
            await Clients.Group(chatRoomId.ToString()).SendAsync("ReceiveMessage", message);
        }

        public static bool IsUserOnline(int userId)
        {
            return UserConnections.TryGetValue(userId, out var conns) && conns.Count > 0;
        }

        public static DateTime? GetLastActive(int userId)
        {
            if (LastActive.TryGetValue(userId, out DateTime last))
                return last;

            return null;
        }

    }
}
