using chat_server.Hubs;
using chat_server.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace chat_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LiveMatchController : ControllerBase
    {
        private readonly IHubContext<ChatHub> _hubContext;

        public LiveMatchController(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        [HttpPost]
        [Route("PushInvitation")]
        public IActionResult PushInvitation(String connectioId, LiveMatch invitation)
        {
            _hubContext.Clients.Client(connectioId).SendAsync("ReceiveLiveInvitation", invitation);

            return Ok("Done");
        }
    }
}
