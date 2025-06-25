using AIDMS.Shared.Application.Interfaces.Services;
using Microsoft.AspNetCore.SignalR;
using OllamaSharp.Models.Chat;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIDMS.Server.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IOllamaService _ollamaService;

        public ChatHub(IOllamaService ollamaService)
        {
            _ollamaService = ollamaService;
        }

        public async Task StreamChat(string user, string query, string context, List<Message> messageHistory, bool streamReasoning)
        {
            await foreach (var token in _ollamaService.StreamResponseAsync(query, context, messageHistory, streamReasoning))
            {
                if (token.Contains("\n\n") || token.Contains("\n"))
                {
                    await Clients.Caller.SendAsync("ReceiveToken", "new_line"); // Send empty token to indicate a new line
                }
                await Clients.Caller.SendAsync("ReceiveToken", token);
            }

            await Clients.Caller.SendAsync("StreamCompleted");
        }
    }
}
