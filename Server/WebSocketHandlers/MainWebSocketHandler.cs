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
            if (_lobbyService.CanAddPlayer() == false)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.PolicyViolation, "Lobby is full", CancellationToken.None);
                Console.WriteLine("Connection attempt rejected: Lobby is full");
                return;
            }

            if (_gameHandler.IsGameStarted())
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.PolicyViolation, "Game is in progress", CancellationToken.None);
                Console.WriteLine("Connection attempt rejected: Game is in progress");
                return;
            }

            var playerId = Guid.NewGuid().ToString();
            _sockets.TryAdd(webSocket, playerId);
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
            } catch (Exception e)
            {
                Console.WriteLine("Failed to receive message from client");
            }
            finally
            {
                if (_gameHandler.IsGameStarted())
                {
                    // remove player from game
                    _gameHandler.RemovePlayer(playerId);
                } else {
                    // remove player from lobby
                    _lobbyService.RemovePlayer(playerId);
                    // broadcast updated lobby state
                    var lobbyState = _lobbyService.GetLobbyState();
                    await BroadcastMessageAsync(lobbyState);
                }

                _sockets.TryRemove(webSocket, out _); // remove WebSocket from collection
                Console.WriteLine($"Player {playerId} disconnected");
            }
        }

        private class CommandMessage
        {
            public string Type { get; set; }
            public string Payload { get; set; }
        }
    }
}