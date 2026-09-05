using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.dtos;
using App.services.interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using OllamaSharp;

namespace App.controllers
{
    [ApiController]
    [Route("api")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            this._chatService = chatService;
        }

        [HttpPost("chat")]
        public async Task<ActionResult<string>> Chat([FromBody] MessageDTO message)
        {
            try
            {
                string response = await this._chatService.Chat(message.Message);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}