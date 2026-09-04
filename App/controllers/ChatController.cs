using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using OllamaSharp;

namespace App.controllers
{
    [ApiController]
    [Route("api")]
    public class ChatController : ControllerBase
    {
        private readonly IChatClient _client;
        private readonly IConfiguration _configuration;

        public ChatController(IConfiguration configuration)
        {
            this._configuration = configuration;

            this._client = new OllamaApiClient(
                new Uri(this._configuration["Ollama:Url"] ?? ""),
                this._configuration["Ollama:EmbeddingModel"] ?? ""
            );
        }

        [HttpPost("chat/streaming")]
        public async Task ChatStreaming([FromBody] string user , CancellationToken cancellationToken)
        {

            Response.ContentType = "text/event-stream";

            ChatOptions options = new ChatOptions
            {
                MaxOutputTokens = 100,
                Temperature = 0.4f,
                TopK = 5    
            };

            await foreach (ChatResponseUpdate response in this._client.GetStreamingResponseAsync(user , options))
            {
                await Response.WriteAsync($"data: {response.Text}\n\n");
                await Response.Body.FlushAsync(cancellationToken);
            }
        }

        [HttpPost("chat")]
        public async Task<ActionResult<string>> Chat([FromBody] string user , CancellationToken cancellationToken)
        {

            ChatOptions options = new ChatOptions
            {
                MaxOutputTokens = 100,
                Temperature = 0.4f,
                TopK = 5    
            };

            ChatResponse response = await this._client.GetResponseAsync(user , options);

            return Ok(response.Text);

        }
    }
}