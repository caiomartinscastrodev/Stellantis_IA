using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace services
{
    public class IngestaoService : IIngestaoService
    {
        private readonly QdrantClient _qdrantClient;
        private readonly IEmbeddingGenerator<string , Embedding<float>> _embeddingGenerator;

        public IngestaoService()
        {
            this._qdrantClient = new QdrantClient(
                "http://localhost",
                6334
            );
            this._embeddingGenerator = new OllamaApiClient(
                new Uri(this._configuration["Ollama:Url"] ?? ""),
                this._configuration["Ollama:ClientModel"] ?? ""
            );
        }
    }
}