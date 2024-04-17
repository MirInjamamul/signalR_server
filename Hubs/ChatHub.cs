using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using chat_server.Models;
using chat_server.Services;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Servers;
using System.Xml.Linq;
using static chat_server.Utils.Util;
using static System.Net.WebRequestMethods;

namespace chat_server.Hubs
{
    public class ChatHub : Hub
    {

        private readonly PresenceTracker _presenceTracker;
        private readonly IRosterService _rosterService;
        private readonly IMessageService _messageService;

        public ChatHub(PresenceTracker presenceTracker, IRosterService rosterService, IMessageService messageService)
        {
            _presenceTracker = presenceTracker;
            _rosterService = rosterService;
            _messageService = messageService;
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

            await base.OnDisconnectedAsync(null);
        }

        public async Task Join(string username)
        {

            var nickName = username;
            string connectionId = Context.ConnectionId;

            var result = await _presenceTracker.ConnectionOpened(nickName);

            // Remove when user went offline
            await _presenceTracker.SetupNickConnection(connectionId, nickName);

            if (result.UserJoined)
            {
                await Clients.All.SendAsync("UserConnected", nickName);
            }

            var currentUsers = await _presenceTracker.GetOnlineUsers();
            await Clients.Caller.SendAsync("onlineUsers", currentUsers);




        }

        private List<OfflineMessageModel> getOfflineMessages(string receiverId)
        {
           return _messageService.GetOfflineMessageByUserAsync(receiverId);
        }

        public async void Connect(string userId)
        {
            var connectionId = Context.ConnectionId;

            UserDetail userDetail = _presenceTracker.Login(connectionId, userId);

            _updatePresence(userId, true);

            List<OfflineMessageModel> offlineMessages = getOfflineMessages(userId);


            foreach (var offlineMessage in offlineMessages)
            {
                MessageModel messageModel = new MessageModel { 
                    SenderId = offlineMessage.Message.SenderId, 
                    SenderUserName = offlineMessage.Message.SenderUserName, 
                    SenderUserPhoto = offlineMessage.Message.SenderUserPhoto, 
                    To = offlineMessage.Message.To, 
                    Message = offlineMessage.Message.Message, 
                    TimeStamp = offlineMessage.Message.TimeStamp,
                };
                await Clients.Caller.SendAsync("ReceiveMessage", messageModel);
            }

            _messageService.deleteMessage(userId);

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

                Roster fromUser = _getUserName(senderUserId);
                Roster toUser = _getUserName(toUserId);

                List<UserDetail> toUserDetail = _presenceTracker.GetUserDetail(toUserId);
                bool[] userStatus = _getUserStatus(senderUserId, toUserId);


                if (toUserDetail.Count != 0)
                {
                    // User is online
                    foreach (UserDetail userDetail in toUserDetail)
                    {
                        if (!userStatus[2]) // Sender is not blocked
                        {
                            MessageModel messageModel = new MessageModel { SenderId = senderUserId, SenderUserName = fromUser.NickName, SenderUserPhoto = fromUser.Photo, To = userDetail.UserId, Message = message };
                            await Clients.Client(userDetail.ConnectionId).SendAsync("ReceiveMessage", messageModel);


                            // TODO save in permanant storage
                            OfflineMessageModel offlineMessageModel = new OfflineMessageModel
                            {
                                Message = new MessageModel
                                {
                                    SenderId = senderUserId,
                                    SenderUserName = fromUser.NickName,
                                    SenderUserPhoto = fromUser.Photo,
                                    To = toUserId,
                                    ReceiverUserName = toUser.NickName,
                                    ReceiverUserPhoto = toUser.Photo,
                                    Message = message
                                },
                                TimeStamp = DateTime.Now,
                                IsOfflineMessage = false
                            };

                            _messageService.InsertOne(offlineMessageModel);

                        }
                        else {
                            Console.WriteLine("Sender is Blocked , Can't send the message");
                        }

                    }
                }
                else {

                    // User is offline

                    if (!userStatus[2]) // Sender is not blocked
                    {
                        // TODO save in storage
                        OfflineMessageModel offlineMessageModel = new OfflineMessageModel
                        {
                            Message = new MessageModel
                            {
                                SenderId = senderUserId,
                                SenderUserName = fromUser.NickName,
                                SenderUserPhoto = fromUser.Photo,
                                To = toUserId,
                                ReceiverUserName = toUser.NickName,
                                ReceiverUserPhoto= toUser.Photo,
                                Message = message
                            },
                            TimeStamp = DateTime.Now,
                            IsOfflineMessage = true
                        };

                        _messageService.InsertOne(offlineMessageModel);
                    }
                    else
                    {
                        Console.WriteLine("Sender is Blocked , Can't send the message");
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

                Roster fromUser = _getUserName(senderUserId);

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
                                        SenderUserName = fromUser.NickName,
                                        SenderUserPhoto = fromUser.Photo,
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

                Roster fromUser = _getUserName(senderUserId);

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
                                SenderUserName = fromUser.NickName,
                                SenderUserPhoto = fromUser.Photo,
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

        public async void SendSpeedDatingMessage(string toUserId, string message)
        {
            try
            {
                string fromConnectionId = Context.ConnectionId;
                string senderUserId = _presenceTracker.GetUserId(fromConnectionId);

                Roster fromUser = _getUserName(senderUserId);

                List<UserDetail> toUserDetail = _presenceTracker.GetUserDetail(toUserId);

                if (toUserDetail.Count != 0)
                {
                    foreach (UserDetail userDetail in toUserDetail)
                    {
                        bool[] userStatus = _getUserStatus(senderUserId, toUserId);

                        MessageModel messageModel = new MessageModel { SenderId = senderUserId, SenderUserName = fromUser.NickName,SenderUserPhoto= fromUser.Photo, To = userDetail.UserId, Message = message };
                        await Clients.Client(userDetail.ConnectionId).SendAsync("ReceiveSpeedDatingMessage", messageModel);
                    }
                }
                else
                {
                    
                    // TODO Need to acknoledge that user is diconnected from network
                    Console.WriteLine($"Receiver {toUserId} is offline in speed dating");
                }

            }
            catch(Exception e) { 
                Console.WriteLine("Speed Dating Crash : "+ e.ToString());
            }
        }

        public async void LeftSpeedDatingMessage(string toUserId, string photo)
        {
            try
            {
                string fromConnectionId = Context.ConnectionId;
                string senderUserId = _presenceTracker.GetUserId(fromConnectionId);

                Roster fromUser = _getUserName(senderUserId);

                List<UserDetail> toUserDetail = _presenceTracker.GetUserDetail(toUserId);

                if (toUserDetail.Count != 0)
                {
                    foreach (UserDetail userDetail in toUserDetail)
                    {
                        bool[] userStatus = _getUserStatus(senderUserId, toUserId);

                        MessageModel messageModel = new MessageModel { SenderId = senderUserId, SenderUserName = fromUser.NickName, SenderUserPhoto= fromUser.Photo, To = userDetail.UserId, Message = photo };
                        await Clients.Client(userDetail.ConnectionId).SendAsync("EndSpeedDating", messageModel);
                    }
                }
                else
                {

                    // TODO Need to acknoledge that user is diconnected from network
                    Console.WriteLine($"Receiver {toUserId} is offline in speed dating");
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Speed Dating Crash : " + e.ToString());
            }
        }


        public Roster _getUserName(string userId) 
        { 
            return _rosterService.Get(userId);
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

            await PostOnlineStatus(userId, onlineStatus);




        }

        static async Task PostOnlineStatus(string userId, bool onlineStatus) 
        {
            string apiURL = "http://185.100.232.17:3011/api/online-status";

            int status = (onlineStatus) ? 1 : 0;

            // payload
            string jsonPayload = $@"{{
                ""id"": ""{userId}"",
                ""online"": {status}
            }}";

            using (HttpClient client = new HttpClient())
            { 
                StringContent content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");

                try
                {
                    HttpResponseMessage responseMessage = await client.PostAsync(apiURL, content);
                    if (responseMessage.IsSuccessStatusCode)
                    {

                    }
                    else
                    {
                        Console.WriteLine($"Error {responseMessage.StatusCode} - {responseMessage.ReasonPhrase}");
                    }
                }
                catch (Exception e) {
                    Console.WriteLine($"Exception: {e.Message}");
                }
            }

        }

        public bool[] _getUserStatus(string senderUserId, string receiverUserId)
        {
            bool []data = new bool[3];
            var receiverRoster = _rosterService.Get(receiverUserId);

            

            if (receiverRoster != null)
            {

                
                data[0] = receiverRoster.IsActive;
                data[1] = true;

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

            LiveCall liveMatch = new LiveCall();
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

        public void SendOffer(String receiverUserId, String senderId, String roomId, String name, String photo, bool isVideo, String message)
        {

            List<UserDetail> reveiverUserDetail = _presenceTracker.GetUserDetail(receiverUserId);

            LiveCall liveCall = new LiveCall();
            liveCall.SenderId = senderId;
            liveCall.RoomId = roomId;
            liveCall.Name = name;
            liveCall.Photo = photo;
            liveCall.Message = message;
            liveCall.IsVideo = isVideo;

            bool[] userStatus = _getUserStatus(senderId, receiverUserId);

            foreach (var item in reveiverUserDetail)
            {

                if (!userStatus[2]) // Sender is not blocked
                {
                    Clients.Client(item.ConnectionId).SendAsync("ReceiveLiveInvitation", liveCall);
                }
                else
                {
                    Console.WriteLine("Sender is Blocked , Can't send the message");
                }

                
            }
        }

        public void SendReject(String receiverUserId, String senderId, String roomId, String name, String photo, bool isVideo, String message)
        {

            List<UserDetail> reveiverUserDetail = _presenceTracker.GetUserDetail(receiverUserId);

            LiveCall rejectCall = new LiveCall();
            rejectCall.SenderId = senderId;
            rejectCall.RoomId = roomId;
            rejectCall.Name = name;
            rejectCall.Photo = photo;
            rejectCall.Message = message;
            rejectCall.IsVideo = isVideo;

            foreach (var item in reveiverUserDetail)
            {
                Clients.Client(item.ConnectionId).SendAsync("ReceiveCallReject", rejectCall);
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

        // Features Functionaly
        public async void SendTypingStatus(String receiverUserId, bool typing)
        {
            try 
            { 
                String connectionId = Context.ConnectionId;
                String senderUserId = _presenceTracker.GetUserId(connectionId);

                List<UserDetail> receiverUserDetail = _presenceTracker.GetUserDetail(receiverUserId);

                foreach(UserDetail receiverUser in receiverUserDetail)
                {
                    TypingModel typingModel = new TypingModel { SenderId = senderUserId, TypingStatus = typing };
                    await Clients.Client(receiverUser.ConnectionId).SendAsync("ReceiveTypingStatus", typingModel);
                }
               

            }
            catch (Exception e) { }
        }
    }
}
