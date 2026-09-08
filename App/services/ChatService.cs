using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.services.interfaces;
using Microsoft.Extensions.AI;
using OllamaSharp;
using Qdrant.Client;
using services.interfaces;

namespace App.services
{
    public class ChatService : IChatService
    {
        private readonly IChatClient _client;
        private readonly IEmbeddingGenerator<string , Embedding<float>> _embeddingGenerator;
        private readonly QdrantClient _vectorStore;
        private readonly IConfiguration _configuration;
        private readonly IIngestaoService _ingestao;

        public ChatService(IConfiguration configuration , IIngestaoService ingestao)
        {
            this._ingestao = ingestao;
            this._configuration = configuration;
            this._client = new OllamaApiClient(
                new Uri(this._configuration["Ollama:Url"] ?? ""),
                this._configuration["Ollama:ClientModel"] ?? ""
            );
            this._embeddingGenerator = new OllamaApiClient(
                new Uri(this._configuration["Ollama:Url"] ?? ""),
                this._configuration["Ollama:EmbeddingModel"] ?? ""
            );
            this._vectorStore = new QdrantClient(
                "localhost",
                6334
            );
        }

        public async Task<string> Chat(string message)
        {
            ChatOptions options = new ChatOptions
            {
                //MaxOutputTokens = 400,
                Temperature = 0.2f,
                TopK = 1,
                
            };

            Embedding<float> embeddedQuestion = await this._embeddingGenerator.GenerateAsync(message);
            ReadOnlyMemory<float> vector = embeddedQuestion.Vector;

            var context = await this._vectorStore.SearchAsync("Documents" , vector , null , null , 1);

            var contextText = string.Join(
                "\n\n",
                context.Select(x => x.Payload["content"].StringValue).ToList()
            );

            List<ChatMessage> messages = new()
            {
                new ChatMessage(
                    ChatRole.System,
                    $"""
                    Você é uma assistente de IA chamada Stella.

                    Você atua como assistente de IA para a empresa Stellantis Financiamento.

                    Responda ao usuário de forma objetiva.

                    IMPORTANTE:
                    - Responda somente com base no CONTEXTO fornecido.
                    - Se a resposta não estiver no contexto, diga que não possui informações suficientes.
                    - Não invente informações.
                    - Responda de forma curta, direta e objetiva.

                    CONTEXTO:
                    {contextText}
                    """
                ),
                new ChatMessage(
                    ChatRole.User,
                    message
                )
            };

            ChatResponse response = await this._client.GetResponseAsync(messages , options);

            return response.Text;
        }

        public async IAsyncEnumerable<string> ChatStreaming(string message)
        {
            ChatOptions options = new ChatOptions
            {
                TopK = 1,
                Temperature = 0.2f,
                //MaxOutputTokens = 400
            };

            Embedding<float> embedding = await this._embeddingGenerator.GenerateAsync(message);
            ReadOnlyMemory<float> vector = embedding.Vector;

            var context = await this._vectorStore.SearchAsync(
                "Documents",
                vector,
                null,
                null,
                2
            );

            string contextText = string.Join(
                "\n\n",
                context.Select(x => x.Payload["content"].StringValue).Where(x => x != "").ToList()
            );

            List<ChatMessage> messages = new()
            {
                new ChatMessage(
                    ChatRole.System,
                    $"""
                    Você é uma assistente de IA chamada Stella.

                    Você atua como assistente de IA para a empresa Stellantis Financiamento.

                    Responda ao usuário de forma objetiva.

                    IMPORTANTE:
                    - Responda somente com base no CONTEXTO fornecido.
                    - Se a resposta não estiver no contexto, diga que não possui informações suficientes.
                    - Não invente informações.
                    - Responda de forma curta, direta e objetiva.

                    CONTEXTO:
                    {contextText}
                    """
                ),
                new ChatMessage(
                    ChatRole.User,
                    message
                )
            };

            await foreach (ChatResponseUpdate response in this._client.GetStreamingResponseAsync(messages , options))
            {
                yield return response.Text;
            }

        }
    }
}