using DATN.Server.Service;
using DATN.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace DATN.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly MessageService _MessageService;

        public MessageController(MessageService _Message)
        {
            _MessageService = _Message;
        }

        [HttpGet("GetMessage")]
        public List<Message> GetMessage()
        {
            return _MessageService.GetMessage();
        }

        [HttpPost("AddMessage")]
        public Message AddMessage(Message Message)
        {
            return _MessageService.AddMessage(new Message
            {
                AccountId = Message.AccountId,
                TableId = Message.TableId,
                MessageText = Message.MessageText,
                Note = Message.Note,
                CreateDate = Message.CreateDate,
                UpdateDate = Message.UpdateDate,
            });
        }

        [HttpGet("{id}")]
        public ActionResult<Message> GetIdMessage(int id)
        {
            if (id == 0)
            {
                return BadRequest("Value must be...");

            }
            return Ok(_MessageService.GetIdMessage(id));
        }


        [HttpDelete("{id}")]
        public IActionResult DeleteMessage(int id)
        {
            var deleted = _MessageService.DeleteMessage(id);
            if (deleted == null)
            {
                return NotFound("Message not found");
            }

            return Ok(deleted);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateMessage(int id, [FromBody] Message updatedMessage)
        {
            var updated = _MessageService.UpdateMessage(id, updatedMessage);
            if (updated == null)
            {
                return NotFound("Message not found");
            }

            return Ok(updated);
        }
    }
}
