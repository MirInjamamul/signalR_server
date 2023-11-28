using chat_server.Hubs;
using chat_server.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver.Core.Connections;
using static chat_server.Utils.Util;

namespace chat_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LiveMatchController : ControllerBase
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly PresenceTracker _presenceTracker;

        public LiveMatchController(IHubContext<ChatHub> hubContext, PresenceTracker presenceTracker)
        {
            _hubContext = hubContext;
            _presenceTracker = presenceTracker;
        }

        [HttpPost]
        [Route("PushInvitation")]
        public IActionResult PushInvitation(String receiverId, LiveMatch invitation)
        {

            List<UserDetail> userDetails = _presenceTracker.GetUserDetail(receiverId);

            foreach (UserDetail userDetail in userDetails)
            {
                _hubContext.Clients.Client(userDetail.ConnectionId).SendAsync("ReceiveLiveInvitation", invitation);
            }


            return Ok("Done");
        }

        [HttpPost]
        [Route("StreamTermination")]
        public IActionResult TerminateStream(String receiverId, int seatType)
        {

            List<UserDetail> userDetails = _presenceTracker.GetUserDetail(receiverId);

            SeatType streamingSeatType;

            switch (seatType) {
                case 1:
                    streamingSeatType = SeatType.single; break;
                case 16:
                    streamingSeatType = SeatType.pk; break;
                default:
                    Console.WriteLine($"SeatType doesn't match {seatType}");
                    streamingSeatType = SeatType.none;
                    break;

            }

            foreach (UserDetail userDetail in userDetails)
            {
                _hubContext.Clients.Client(userDetail.ConnectionId).SendAsync("ReceiveLiveInvitation", streamingSeatType);
            }


            return Ok("Done");
        }
    }
}
