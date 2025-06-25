using AIDMS.Shared.Application.Configurations;
using AIDMS.Shared.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using OllamaSharp;
using OllamaSharp.Models.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AIDMS.Shared.Infrastructure.Services
{
    public class OllamaService : IOllamaService
    {
        private readonly OllamaConfig _ollamaConfig;
        private readonly OllamaApiClient _ollamaClient;
        private readonly HttpClient _client = new();

        public OllamaService(IConfiguration configuration)
        {
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                AllowTrailingCommas = true,
                NewLine = Environment.NewLine
            };
            _ollamaConfig = configuration.GetSection(nameof(OllamaConfig)).Get<OllamaConfig>();
            _ollamaClient = new OllamaApiClient(_ollamaConfig.OllamaUrl, _ollamaConfig.InferenceModel);
        }

        public async IAsyncEnumerable<string> StreamResponseAsync(string context, string query, List<Message> messageHistory, bool streamReasoning)
        {
            await EnsureModelRunninAsync();

            var systemMessage = new Message(ChatRole.System,
                "You are a friendly and knowledgeable university assistant. " +
                "Provide clear, accurate, and concise answers based only on the provided context. " +
                "If the answer is not in the context, politely respond with 'I'm not sure based on the information provided.' " +
                "If the user is greeting you, be polite and greet him back, or ask questions that you are able to answer, be kind to answer and let them know your limitations." +
                "Be helpful and approachable in your tone.");

            // Prepare full message list
            var messages = new List<Message> { systemMessage };

            if (messageHistory != null && messageHistory.Count > 0)
                messages.AddRange(messageHistory);

            messages.Add(new Message(ChatRole.User, $"Context:\n {context} \n\n Question: {query}"));

            var chat = new ChatRequest
            {
                Messages = messages,
                Options = new OllamaSharp.Models.RequestOptions
                {
                    Temperature = _ollamaConfig.Temperature,
                    TopK = _ollamaConfig.TopK,
                    TopP = _ollamaConfig.TopP,
                    NumBatch = _ollamaConfig.NumContext,
                    NumPredict = _ollamaConfig.MaxTokens,
                    NumThread = _ollamaConfig.NumCpus
                }
            };

            await foreach (var token in _ollamaClient.ChatAsync(chat))
            {
                if (!streamReasoning)
                {
                    if ((bool)(token.Message?.Content.Contains("</think>")))
                        streamReasoning = true;
                    continue;
                }

                yield return token.Message.Content;
            }
        }

        private async Task EnsureModelRunninAsync()
        {
            var runningModels = await _ollamaClient.ListRunningModelsAsync();
            if (!runningModels.Any(m => m.Name.Equals(_ollamaConfig.InferenceModel, StringComparison.OrdinalIgnoreCase)))
            {
                _ollamaClient.SelectedModel = _ollamaConfig.InferenceModel;
                var chat = new ChatRequest
                {
                    Messages = new List<Message>
                    {
                        new Message(ChatRole.System, "")
                    },
                    Options = new OllamaSharp.Models.RequestOptions
                    {
                        NumPredict = 1
                    }
                };
                await foreach (var answerToken in _ollamaClient.ChatAsync(chat))
                {

                }
            }
        }

        public async Task<float[]> GenerateEmbeddingAsync(string text)
        {
            var payload = new
            {
                model = _ollamaConfig.EmbedingModel,
                prompt = text
            };

            var response = await _client.PostAsJsonAsync("http://localhost:11434/api/embeddings", payload);
            response.EnsureSuccessStatusCode();

            using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            var embedding = json.RootElement.GetProperty("embedding")
                .EnumerateArray()
                .Select(x => x.GetSingle())
                .ToArray();

            return embedding;
        }
    }
}