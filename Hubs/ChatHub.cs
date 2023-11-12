using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using chat_server.Models;
using chat_server.Services;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver.Core.Connections;
using static chat_server.Utils.Util;

namespace chat_server.Hubs
{
    public class ChatHub : Hub
    {

        private readonly PresenceTracker _presenceTracker;
        private readonly IRosterService _rosterService;

        public ChatHub(PresenceTracker presenceTracker, IRosterService rosterService)
        {
            _presenceTracker = presenceTracker;
            _rosterService = rosterService;
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
            //var result = await _presenceTracker.ConnectionClosed(user);

            //if (result.UserLeft)
            //{
            //    await Clients.All.SendAsync("UserDisconnected", "User Disconnected");
           // }

            // Broadcast online users to All
           // var currentUsers = await _presenceTracker.GetOnlineUsers();
           // await Clients.All.SendAsync("onlineUsers", currentUsers);

            var item = _presenceTracker.Logout(user);

            _updatePresence(item.UserId, false);

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

            _updatePresence(userId, true);

            await Clients.All.SendAsync("UserLogged", userDetail);
        }

        public void BroadcastUser(User user)
        {
            Clients.All.SendAsync("ReceiveUser", user);
        }

        // Send to one - one mesage

        public async void SendPrivateMessage(string toUserId, string message)
        {
            try
            {
                string fromConnectionId = Context.ConnectionId;
                string senderUserId = _presenceTracker.GetUserId(fromConnectionId);

                string fromUsername = _getUserName(senderUserId);

                List<UserDetail> toUserDetail = _presenceTracker.GetUserDetail(toUserId);

                if (toUserDetail.Count != 0)
                {
                    foreach (UserDetail userDetail in toUserDetail)
                    {
                        bool []userStatus = _getUserStatus(senderUserId, toUserId);
                        if (!userStatus[2]) // not blocked
                        {
                            if (userStatus[0]) // receiver is online or not
                            {
                                if (userStatus[1]) // request message or not
                                {
                                    MessageModel messageModel = new MessageModel { SenderId = senderUserId, SenderUserName = fromUsername, To = userDetail.UserId, Message = message };
                                    await Clients.Client(userDetail.ConnectionId).SendAsync("ReceiveMessage", messageModel);
                                }
                                else
                                {
                                    MessageModel messageModel = new MessageModel { SenderId = senderUserId, SenderUserName = fromUsername, To = userDetail.UserId, Message = message };
                                    await Clients.Client(userDetail.ConnectionId).SendAsync("ReceiveRequestMessage", messageModel);
                                }

                            }
                            else
                            {
                                // TODO send push notification

                            }
                        }
                        
                        
                    }
                }

            }
            catch { }
        }

        public async void SendPrivateFileMessage(string toUserId, string title, string url)
        {
            try
            {
                string fromConnectionId = Context.ConnectionId;
                string senderUserId = _presenceTracker.GetUserId(fromConnectionId);

                string fromUsername = _getUserName(senderUserId);

                List<UserDetail> toUserDetail = _presenceTracker.GetUserDetail(toUserId);

                if (toUserDetail.Count != 0)
                {
                    foreach (UserDetail userDetail in toUserDetail)
                    {
                        bool[] userStatus = _getUserStatus(senderUserId, toUserId);
                        if (!userStatus[2]) // not blocked
                        {
                            if (userStatus[0]) // receiver is online or not
                            {
                                FileMessageModel messageModel = new FileMessageModel
                                {
                                    Message = new MessageModel
                                    {
                                        SenderId = senderUserId,
                                        SenderUserName = fromUsername,
                                        To = userDetail.UserId,
                                        Message = title
                                    },
                                    Url = url
                                };

                                if (userStatus[1]) // request message or not
                                {
                                    await Clients.Client(userDetail.ConnectionId).SendAsync("ReceiveFileMessage", messageModel);
                                }
                                else
                                {
                                    await Clients.Client(userDetail.ConnectionId).SendAsync("ReceiveRequestFileMessage", messageModel);
                                }

                            }
                            else
                            {
                                // TODO send push notification

                            }
                        }


                    }
                }

            }
            catch { }
        }

        public async void SendStoryReaction(string toUserId, string message, string storyLink, StoryType storyType)
        {
            try
            {
                string fromConnectionId = Context.ConnectionId;
                string senderUserId = _presenceTracker.GetUserId(fromConnectionId);

                string fromUsername = _getUserName(senderUserId);

                List<UserDetail> toUserDetail = _presenceTracker.GetUserDetail(toUserId);

                if (toUserDetail.Count != 0)
                {
                    foreach (UserDetail userDetail in toUserDetail)
                    {

                        StoryReactionModel storyReactionModel = new StoryReactionModel
                        {
                            Message = new MessageModel
                            {
                                SenderId = senderUserId,
                                SenderUserName = fromUsername,
                                To = userDetail.UserId,
                                Message = message
                            },
                            StoryLink = storyLink,
                            StoryType = storyType
                        };
                        
                        await Clients.Client(userDetail.ConnectionId).SendAsync("ReceiveStoryReaction", storyReactionModel);

                    }
                }

            }
            catch { }
        }

        public string _getUserName(string userId) 
        { 
            var userDetails = _rosterService.Get(userId);

            return userDetails.NickName;
        }

        public async void _updatePresence(string userId, bool onlineStatus)
        { 
            var roster = _rosterService.Get(userId); 
            
            if (roster != null) 
            { 
                roster.IsActive = onlineStatus;
                roster.LastOnline = DateTime.Now;

                _rosterService.Update(userId, roster);
            }
        }

        public bool[] _getUserStatus(string senderUserId, string receiverUserId)
        {
            bool []data = new bool[3];
            var receiverRoster = _rosterService.Get(receiverUserId);

            if (receiverRoster != null)
            {
                data[0] = receiverRoster.IsActive;

                if (receiverRoster.Follower.Any(follower => follower.UserId == senderUserId))
                {
                    data[1] = true;
                }
                else 
                {
                    data[1] = false;
                }

                if (receiverRoster.Blocked != null && receiverRoster.Blocked.Contains(senderUserId))
                {
                    data[2] = true;
                }
                else
                {
                    data[2] = false;
                }
            }
            else
            {
                data[0] = false;
                data[1] = false;
                data[2] = false;
                
            }

            return data;
        }

        // Send to one - one Live Match Invitation mesage
        public void LiveInviteToUser(String receiverUserId, String senderId, String roomId, String name, String photo, String message)
        {

            List<UserDetail> reveiverUserDetail = _presenceTracker.GetUserDetail(receiverUserId);

            LiveMatch liveMatch = new LiveMatch();
            liveMatch.SenderId = senderId;
            liveMatch.RoomId = roomId;
            liveMatch.Name = name;
            liveMatch.Photo = photo;
            liveMatch.Message = message;

            foreach (var item in reveiverUserDetail)
            {
                Clients.Client(item.ConnectionId).SendAsync("ReceiveLiveInvitation", liveMatch);
            }            
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
