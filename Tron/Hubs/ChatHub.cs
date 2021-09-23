using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Tron.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string name, string text)
        {
            var message = new ChatMessage
            {
                SenderName = name,
                Text = text,
                SendAt = DateTimeOffset.UtcNow,
            };
            // invoke this ReceiveMessage method in the client
            // Broadcast to all clients
            await Clients.All.SendAsync(
                    "ReceiveMessage",
                    message.SenderName,
                    message.Text,
                    message.SendAt
                );
        }

    }

    internal class ChatMessage
    {
        public string SenderName { get; set; }
        public string Text { get; set; }
        public DateTimeOffset SendAt { get; set; }
    }
}
