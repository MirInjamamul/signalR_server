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

        public HomeController(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
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
    }
}
