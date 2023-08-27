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
        public async Task LiveInviteToUser(String connectionId, LiveMatch liveMatchInvitation)
        {
            await Clients.Client(connectionId).SendAsync("ReceiveLiveInvitation", liveMatchInvitation);
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
