using chat_server.Models;
using Microsoft.AspNetCore.SignalR;

namespace chat_server.Hubs
{
    public class ChatHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await Clients.All.SendAsync("UserConnected", Context.ConnectionId);

            await base.OnConnectedAsync();
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
