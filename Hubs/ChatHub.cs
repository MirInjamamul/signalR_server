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
            var user = Context.ConnectionId;
            //var result =  await _presenceTracker.ConnectionOpened(user);
            /*
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
            */

            await Clients.Caller.SendAsync("ConnectionId", Context.ConnectionId);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var user = Context.ConnectionId;
            var result = await _presenceTracker.ConnectionClosed(user);

            if (result.UserLeft)
            {
                await Clients.All.SendAsync("UserDisconnected", "User Disconnected");
            }

            // Broadcast online users to All
            var currentUsers = await _presenceTracker.GetOnlineUsers();
            await Clients.All.SendAsync("onlineUsers", currentUsers);

            var item = _presenceTracker.Logout(user);

            await Clients.All.SendAsync("UserLoggedOut", item);

            await base.OnDisconnectedAsync(exception);
        }

        public async Task Join(string username)
        {

            var nickName = username;
            string connectionId = Context.ConnectionId;

 /*           var nickAvailable = await _presenceTracker.NickNameAvailable(nickName);
            if (!nickAvailable) 
            {
                nickName = $"{username}_{Guid.NewGuid().ToString("N").Substring(0, 6)}";
            }
 */
            //await _presenceTracker.ConnectionClosed(connectionId);

            var result = await _presenceTracker.ConnectionOpened(nickName);

            // Remove when user went offline
            await _presenceTracker.SetupNickConnection(connectionId, nickName);

            if(result.UserJoined)
            {
                await Clients.All.SendAsync("UserConnected", nickName);
            }

            var currentUsers = await _presenceTracker.GetOnlineUsers();
            await Clients.Caller.SendAsync("onlineUsers", currentUsers);

        }

        public async void Connect(string userId)
        {
            var connectionId = Context.ConnectionId;

            UserDetail userDetail = _presenceTracker.Login(connectionId, userId);

            await Clients.All.SendAsync("UserLogged", userDetail);
        }

        public void BroadcastUser(User user)
        {
            Clients.All.SendAsync("ReceiveUser", user);
        }

        // Send to one - one mesage
        public async void BroadcastMessage(string senderId, string receiverId, string message)
        {
            var connectionId = await _presenceTracker.GetConnectionIdByUserId(receiverId);

            MessageModel messageModel = new MessageModel();
            messageModel.Message = message;
            messageModel.From = senderId;
            messageModel.To = receiverId;

            await Clients.Client(connectionId).SendAsync("ReceiveMessage", messageModel);
        }

        public async void SendPrivateMessage(string toUserId, string message)
        {
            try { 
                string fromConnectionId = Context.ConnectionId;
                string fromUserId = _presenceTracker.GetUserId(fromConnectionId);

                List<UserDetail> toUserDetail = _presenceTracker.GetUserDetail(toUserId);

                if (toUserDetail.Count != 0) 
                {
                    foreach (UserDetail userDetail in toUserDetail)
                    {
                        await Clients.Client(userDetail.ConnectionId).SendAsync("ReceiveMessage", message);
                    }
                }

            }catch { }
        }

        // Send to one - one Live Match Invitation mesage
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
