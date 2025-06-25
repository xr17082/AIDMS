using AIDMS.Shared.Application.Interfaces.Services;
using AIDMS.Shared.Application.Responses;
using AIDMS.Shared.Application.Responses.Identity;
using AIDMS.Shared.Wrapper;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Schema;
using UglyToad.PdfPig;

namespace AIDMS.Shared.Infrastructure.Services
{
    public class DocumentProcessor : IDocumentProcessor, IDataInitializer
    {
        private readonly IOllamaService _ollamaService;
        private readonly QdrantClient _qdrantClient;
        private readonly string _basePath = "C:\\Users\\Xhemil\\source\\repos\\ai_dms\\AIDMS\\Server\\Files\\Docs";
        private readonly string _trackingFile = "processed_docs.json";
        private Dictionary<string, List<string>> _processedFiles;
        private const int EmbeddingDimension = 384;

        public DocumentProcessor(IOllamaService ollamaService)
        {
            _ollamaService = ollamaService;
            _qdrantClient = new QdrantClient("localhost", 6334);
           
            LoadProcessedFiles();
        }

        public void Initialize()
        {
            foreach (var deptDir in Directory.GetDirectories(_basePath))
            {
                var deptName = Path.GetFileName(deptDir);
                Console.WriteLine($"Processing department: {deptName}");

                if (!_processedFiles.ContainsKey(deptName))
                    _processedFiles[deptName] = new List<string>();

                EnsureCollectionExistsAsync(deptName).GetAwaiter().GetResult();

                var files = Directory.GetFiles(deptDir, "*.*", SearchOption.AllDirectories)
                                     .Where(f => f.EndsWith(".pdf") || f.EndsWith(".txt"));

                foreach (var file in files)
                {
                    if (_processedFiles[deptName].Contains(file))
                    {
                        Console.WriteLine($"Skipping already processed file: {file}");
                        continue;
                    }

                    try
                    {
                        if (file.EndsWith(".pdf"))
                        {
                            var pages = ExtractTextFromPdf(file);
                            var chunkedVectors = new List<(float[] Vector, string Text, int PageNumber)>();

                            foreach (var (text, pageNumber) in pages)
                            {
                                var chunks = SplitTextIntoChunks(text);
                                foreach (var chunk in chunks)
                                {
                                    var vector = VectorizeText(chunk).GetAwaiter().GetResult();
                                    chunkedVectors.Add((vector, chunk, pageNumber));
                                }
                            }

                            InsertVector(deptName, file, chunkedVectors).GetAwaiter().GetResult();
                        }
                        else if (file.EndsWith(".txt"))
                        {
                            var text = File.ReadAllText(file);
                            var chunkedVectors = new List<(float[] Vector, string Text, int PageNumber)>();

                            var chunks = SplitTextIntoChunks(text);
                            foreach (var chunk in chunks)
                            {
                                var vector = VectorizeText(chunk).GetAwaiter().GetResult();
                                chunkedVectors.Add((vector, chunk, 1));
                            }

                            InsertVector(deptName, file, chunkedVectors).GetAwaiter().GetResult();
                        }

                        _processedFiles[deptName].Add(file);
                        SaveProcessedFiles();
                        Console.WriteLine($"Processed and stored file: {file}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing {file}: {ex.Message}");
                    }
                }
            }
        }

        public async Task<Result<List<SearchDocumentsResponse>>> SearchDocuments(string query)
        {
            var queryEmbedings = await _ollamaService.GenerateEmbeddingAsync(query);
            var context = new List<SearchDocumentsResponse>();
            var result = await _qdrantClient.SearchAsync("Law", queryEmbedings, limit: 5, scoreThreshold: 0.25f);
            foreach (var item in result)
            {
                context.Add(new SearchDocumentsResponse
                {
                    FileName = item.Payload["filename"].StringValue,
                    PageNumber = (int)item.Payload["page_number"].IntegerValue,
                    Text = item.Payload["text"].StringValue,
                    Score = item.Score
                });
            }
            return await Result<List<SearchDocumentsResponse>>.SuccessAsync(context);
        }

        private void LoadProcessedFiles()
        {
            if (File.Exists(_trackingFile))
            {
                var json = File.ReadAllText(_trackingFile);
                _processedFiles = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json)
                                  ?? new Dictionary<string, List<string>>();
            }
            else
            {
                _processedFiles = new Dictionary<string, List<string>>();
            }
        }

        private void SaveProcessedFiles()
        {
            var json = JsonSerializer.Serialize(_processedFiles, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_trackingFile, json);
        }

        private async Task EnsureCollectionExistsAsync(string department)
        {
            var exists = await _qdrantClient.CollectionExistsAsync(department);
            if (!exists)
            {
                await _qdrantClient.CreateCollectionAsync(department, new VectorParams { Size = EmbeddingDimension, Distance = Distance.Cosine });
            }
        }

        private List<(string Text, int PageNumber)> ExtractTextFromPdf(string filePath)
        {
            var result = new List<(string, int)>();
            using var document = PdfDocument.Open(filePath);
            foreach (var page in document.GetPages())
            {
                result.Add((page.Text, page.Number));
            }
            return result;
        }

        private async Task<float[]> VectorizeText(string text)
        {
            return await _ollamaService.GenerateEmbeddingAsync(text);
        }

        private async Task InsertVector(string collectionName, string filename, List<(float[] Vector, string Text, int PageNumber)> chunkedData)
        {
            var points = new List<PointStruct>();

            foreach (var (vector, text, pageNumber) in chunkedData)
            {
                var payload = new Dictionary<string, Value>
                {
                    ["text"] = new Value { StringValue = text },
                    ["filename"] = new Value { StringValue = filename },
                    ["page_number"] = new Value { IntegerValue = pageNumber }
                };

                var point = new PointStruct
                {
                    Id = new PointId { Uuid = Guid.NewGuid().ToString("N") },
                    Vectors = new() { Vector = vector }
                };
                point.Payload.Add(payload);
                points.Add(point);
            }

            var result = await _qdrantClient.UpsertAsync(collectionName, points);
            Console.WriteLine(result.Status);
        }

        private IEnumerable<string> SplitTextIntoChunks(string text, int chunkSize = 500, int overlap = 50)
        {
            if (string.IsNullOrWhiteSpace(text))
                yield break;

            int start = 0;
            while (start < text.Length)
            {
                int length = Math.Min(chunkSize, text.Length - start);
                yield return text.Substring(start, length);
                start += chunkSize - overlap;
            }
        }
    }
}