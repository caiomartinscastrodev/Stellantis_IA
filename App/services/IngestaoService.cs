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
using UglyToad.PdfPig;
using UglyToad.PdfPig.Actions;
using UglyToad.PdfPig.Content;

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
                "localhost",
                6334
            );
            this._embeddingGenerator = new OllamaApiClient(
                new Uri(this._configuration["Ollama:Url"] ?? ""),
                this._configuration["Ollama:EmbeddingModel"] ?? ""
            );
            this._files = new List<string>
            {
                "Alteração Cadastral.pdf",
                "Alterações de Contrato.pdf",
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
            if (!await this._vectorStore.CollectionExistsAsync("Documents"))
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
                var PathBase = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "documents\\"
                );

                foreach (string file in this._files)
                {

                    PdfDocument pdf = PdfDocument.Open(PathBase + file);

                    string DocumentCompleted = "";

                    foreach (Page page in pdf.GetPages())
                    {
                        DocumentCompleted = DocumentCompleted + page.Text;
                    }

                    List<string> chunks = DocumentCompleted.Split("---DOCUMENTO---").Where(x => x != "").Select(x => x.Trim()).ToList();

                    foreach (string chunk in chunks)
                    {
                        Console.WriteLine($"{chunk}\n\n");
                    }

                    foreach (string data in chunks)
                    {
                        Embedding<float> embedding = await this._embeddingGenerator.GenerateAsync(data);
                        ReadOnlyMemory<float> vector = embedding.Vector;
                        float[] array = vector.ToArray();


                        List<PointStruct> points = new List<PointStruct>
                        {
                            new PointStruct
                            {
                                Id = Guid.NewGuid(),
                                Vectors = array,
                                Payload =
                                {
                                    ["title"] = file.Split(".").FirstOrDefault() ?? "",
                                    ["content"] = data
                                }
                            }
                        };

                        await this._vectorStore.UpsertAsync("Documents" , points);
                    }
                }
            }
        }
    }
}