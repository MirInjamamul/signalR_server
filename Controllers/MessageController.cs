using chat_server.Models;
using chat_server.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace chat_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public MessageController(IMessageService messageService) 
        { 
            _messageService = messageService;
        }

        // Get BackUp Messages
        [HttpGet]
        public ActionResult<List<OfflineMessageModel>> GetBackupMessage(String userId) 
        { 
            var messages = _messageService.GetBackupMessageByUser(userId);

            return messages;
        }
    }
}
