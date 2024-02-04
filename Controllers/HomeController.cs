using chat_server.Hubs;
using chat_server.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace chat_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly PresenceTracker _presenceTracker;

        public HomeController(IHubContext<ChatHub> hubContext, PresenceTracker presenceTracker)
        {
            _hubContext = hubContext;
            _presenceTracker = presenceTracker;
        }

        [HttpPost]
        [Route("PushUser")]
        public IActionResult PushUser(int id, string name) {
            User user = new User();
            user.Id = id;
            user.Name = name;

            _hubContext.Clients.All.SendAsync("ReceiveUser", user);

            return Ok("Done");
        }

        [HttpPost]
        [Route("PushMessage")]
        public IActionResult PushMessage(string message)
        {
            _hubContext.Clients.All.SendAsync("ReceiveMessage", message);

            return Ok("Done");
        }

        [HttpPost]
        [Route("PushOfficialMessage")]
        public IActionResult PushOfficialMessage(string receiverId, string message, string mediaUrl, int messageType) {
            List<UserDetail> userDetails = _presenceTracker.GetUserDetail(receiverId);

            foreach (UserDetail userDetail in userDetails) { 
                _hubContext.Clients.Client(userDetail.ConnectionId).SendAsync("ReceiveOfficialMessage",message, mediaUrl, messageType);
            }
            
            return Ok("Done");
        }
    }
}
