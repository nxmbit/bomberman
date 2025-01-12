using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Collections.Concurrent;

namespace Bomberman.Server.WebSocketHandlers
{
    public abstract class BaseHandler
    {
        protected static readonly ConcurrentDictionary<WebSocket, string> _sockets = new ConcurrentDictionary<WebSocket, string>();

        protected async Task SendMessageAsync(WebSocket socket, object message)
        {
            var json = JsonSerializer.Serialize(message);
            var buffer = Encoding.UTF8.GetBytes(json);
            Console.WriteLine("Broadcasting message: " + json);
            await socket.SendAsync(
                new ArraySegment<byte>(buffer),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
            );
        }

        protected async Task BroadcastMessageAsync(object message)
        {
            foreach (var socket in _sockets.Keys)
            {
                await SendMessageAsync(socket, message);
            }
        }
    }
}