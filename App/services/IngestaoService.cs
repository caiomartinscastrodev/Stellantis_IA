using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using OllamaSharp;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using services.interfaces;

namespace services
{
    public class IngestaoService : IIngestaoService
    {
        private readonly QdrantClient _vectorStore;
        private readonly IEmbeddingGenerator<string , Embedding<float>> _embeddingGenerator;
        private readonly IConfiguration _configuration;
        private readonly List<string> _files;

        public IngestaoService(IConfiguration configuration)
        {
            this._configuration = configuration;
            this._vectorStore = new QdrantClient(
                "http://localhost",
                6334
            );
            this._embeddingGenerator = new OllamaApiClient(
                new Uri(this._configuration["Ollama:Url"] ?? ""),
                this._configuration["Ollama:EmbeddingModel"] ?? ""
            );
            this._files = new List<string>
            {
                "Alteração Cadastral.pdf",
                "Alteração de Contrato.pdf",
                "Boletos e Pagamentos.pdf",
                "Cadastro Sou Cliente.pdf",
                "Documentos Financiamento.pdf",
                "Informações Cobrança.pdf",
                "Informações DDA.pdf",
                "Informações Financiamento.pdf",
                "Informações Gravame.pdf"
            };
        }

        public async Task Ingestao()
        {
            if (await this._vectorStore.CollectionExistsAsync("Documents"))
            {
                await this._vectorStore.CreateCollectionAsync(
                    "Documents",
                    new VectorParams
                    {
                        Size = 768,
                        Distance = Distance.Cosine
                    }
                );
            }

            if (this._configuration["Ingestao:FazerIngestao"] == "true")
            {
                string PathBase = Path.Combine(
                    AppContext.BaseDirectory,
                    "documents"
                );

                foreach (string file in this._files)
                {
                    
                }
            }
        }
    }
}