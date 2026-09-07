using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.dtos;
using App.services.interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using OllamaSharp;
using services.interfaces;

namespace App.controllers
{
    [ApiController]
    [Route("api")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IIngestaoService _ingestaoService;

        public ChatController(IChatService chatService , IIngestaoService ingestaoService)
        {
            this._chatService = chatService;
            this._ingestaoService = ingestaoService;
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

        [HttpPost("chatstreaming")]
        public async Task ChatStreaming([FromBody] MessageDTO message)
        {
            try
            {
                Response.ContentType = "text/event-stream";

                await foreach (string data in this._chatService.ChatStreaming(message.Message))
                {
                    await Response.WriteAsync($"{data}");
                    await Response.Body.FlushAsync();
                }
            }
            catch (Exception ex)
            {
                await Response.WriteAsync( $"event: error\ndata: {ex.Message}\n\n" ); await Response.Body.FlushAsync();
            }
        }

        [HttpGet("ingestao")]
        public async Task<ActionResult> Ingestao()
        {
            try
            {
                await this._ingestaoService.Ingestao();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}