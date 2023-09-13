using chat_server.Models;
using Microsoft.AspNetCore.SignalR;

namespace chat_server.Hubs
{
    public class ChatHub : Hub
    {

        private readonly PresenceTracker _presenceTracker;

        public ChatHub(PresenceTracker presenceTracker)
        {
            _presenceTracker = presenceTracker;
        }

        public override async Task OnConnectedAsync()
        {
            var user = Context.User.Identity.Name ?? Context.ConnectionId;
            var result =  await _presenceTracker.ConnectionOpened(user);

            if (result.UserJoined)
            {
                // Send Notice to Caller Only
                await Clients.Caller.SendAsync("signalRConnected", "Connected to server");

                // Send Notice to everyone
                //await Clients.All.SendAsync("UserConnected", Context.ConnectionId);

                // Broadcase online users to everyone
                //var currentUsers = await _presenceTracker.GetOnlineUsers();
                //await Clients.All.SendAsync("onlineUsers", currentUsers);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var user = Context.User.Identity.Name ?? Context.ConnectionId;
            var result = await _presenceTracker.ConnectionClosed(user);

            if (result.UserLeft)
            {
                await Clients.All.SendAsync("UserDisconnected", "User Disconnected");
            }

            // Broadcast online users to All
            var currentUsers = await _presenceTracker.GetOnlineUsers();
            await Clients.All.SendAsync("onlineUsers", currentUsers);

            await base.OnDisconnectedAsync(exception);
        }

        public async Task Join(string username, string connecitonId)
        {

            var nickName = username;

 /*           var nickAvailable = await _presenceTracker.NickNameAvailable(nickName);
            if (!nickAvailable) 
            {
                nickName = $"{username}_{Guid.NewGuid().ToString("N").Substring(0, 6)}";
            }
 */
            await _presenceTracker.ConnectionClosed(connecitonId);

            var result = await _presenceTracker.ConnectionOpened(nickName);

            // Remove when user went offline
            await _presenceTracker.SetupNickConnection(connecitonId, nickName);

            if(result.UserJoined)
            {
                await Clients.All.SendAsync("UserConnected", nickName);
            }

            var currentUsers = await _presenceTracker.GetOnlineUsers();
            await Clients.All.SendAsync("onlineUsers", currentUsers);

        }

        public void BroadcastUser(User user)
        {
            Clients.All.SendAsync("ReceiveUser", user);
        }

        public void BroadcastMessage(string message)
        {
            Clients.All.SendAsync("ReceiveMessage", message);
        }

        // Send to one - one mesage
        public void LiveInviteToUser(String connectionId, String senderId, String roomId, String name, String photo, String message)
        {
            LiveMatch liveMatch = new LiveMatch();
            liveMatch.SenderId = senderId;
            liveMatch.RoomId = roomId;
            liveMatch.Name = name;
            liveMatch.Photo = photo;
            liveMatch.Message = message;

            Clients.Client(connectionId).SendAsync("ReceiveLiveInvitation", liveMatch);
        }


        public void LiveMessageToUser(String connectionId, String message)
        {
            Clients.Client(connectionId).SendAsync("ReceiveLiveMessage", message);
        }


        public string GetConnectionId()
        {
            return Context.ConnectionId;
        }
    }
}
