using OllamaSharp.Models.Chat;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIDMS.Shared.Application.Interfaces.Services
{
    public interface IOllamaService
    {
        Task<float[]> GenerateEmbeddingAsync(string text);

        IAsyncEnumerable<string> StreamResponseAsync(string context, string query, List<Message> messageHistory, bool streamReasoning);
    }
}
