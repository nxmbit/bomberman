using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Bomberman.Server.GameLogic;

namespace Bomberman.Server.WebSocketHandlers
{
    public class MainWebSocketHandler : BaseHandler
    {
        private readonly GameHandler _gameHandler;
        private readonly LobbyHandler _lobbyHandler;
        private readonly LobbyService _lobbyService;

        public MainWebSocketHandler(LobbyService lobbyService, GameService gameService)
        {
            _lobbyService = lobbyService;
            _gameHandler = new GameHandler(gameService);
            _lobbyHandler = new LobbyHandler(lobbyService, _gameHandler);

        }

        public async Task HandleAsync(WebSocket webSocket)
        {
            var playerId = Guid.NewGuid().ToString();
            _sockets.Add(webSocket);
            Console.WriteLine($"Player {playerId} connected");

            var buffer = new byte[1024 * 4];

            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                        break;

                    var msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine(msg);
                    var command = JsonSerializer.Deserialize<CommandMessage>(msg);

                    Console.WriteLine(command.Type);

                    switch (command.Type)
                    {
                        case string type when type.StartsWith("CLIENT_LOBBY"):
                            await _lobbyHandler.HandleAsync(playerId, command.Type, command.Payload, webSocket);
                            break;
                        case string type when type.StartsWith("CLIENT_GAME"):
                            await _gameHandler.HandleAsync(playerId, command.Type, command.Payload, webSocket);
                            break;
                    }
                }
            }
            finally
            {
                _lobbyService.RemovePlayer(playerId); // remove player from lobby
            }
        }

        private class CommandMessage
        {
            public string Type { get; set; }
            public string Payload { get; set; }
        }
    }
}
