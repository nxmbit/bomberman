using Bomberman.Server.GameLogic;
using System.Net.WebSockets;

namespace Bomberman.Server.WebSocketHandlers
{
    public class LobbyHandler : BaseHandler
    {
        private readonly LobbyService _lobbyService;
        private readonly GameHandler _gameHandler;

        public LobbyHandler(LobbyService lobbyService, GameHandler gameHandler)
        {
            _lobbyService = lobbyService;
            _gameHandler = gameHandler;
        }

        public async Task HandleAsync(string playerId, string type, string payload, WebSocket socket)
        {
            switch (type)
            {
                case ClientCommandType.CLIENT_LOBBY_JOIN:
                    Console.WriteLine("type: " + type);
                    Console.WriteLine("payload: " + payload);
                    if (_lobbyService.CanAddPlayer()) {
                        _lobbyService.AddPlayer(playerId, payload);
                        var response = new {
                            Type = ServerCommandType.SERVER_LOBBY_JOIN,
                            Payload = new {Response = "OK", PlayerId = playerId}
                        };
                        await SendMessageAsync(socket, response);
                        Console.WriteLine($"Player {playerId} joined the lobby");
                        await BroadcastLobbyState();
                    } else {
                        var response = new {
                            Type = ServerCommandType.SERVER_LOBBY_JOIN,
                            Payload = new {Response = "FULL", PlayerId = playerId}
                        };
                        await SendMessageAsync(socket, response);
                    }
                    break;

                case ClientCommandType.CLIENT_LOBBY_READY:
                    _lobbyService.SetPlayerReady(playerId);
                    await BroadcastLobbyState();
                    if (_lobbyService.AreAllPlayersReady())
                    {
                        await SendMessageAsync(socket, new { Type = ServerCommandType.SERVER_GAME_START });
                        Console.WriteLine(_lobbyService.GetPlayers());
                        _gameHandler.startGame(_lobbyService.GetPlayers());
                    }
                    break;

                case ClientCommandType.CLIENT_LOBBY_UNREADY:
                    _lobbyService.SetPlayerUnready(playerId);
                    await BroadcastLobbyState();
                    break;

                case ClientCommandType.CLIENT_LOBBY_CHANGE_NAME:
                    _lobbyService.SetPlayerName(playerId, payload);
                    await BroadcastLobbyState();
                    break;
                
            }
        }

        private async Task BroadcastLobbyState()
        {
            var lobbyState = _lobbyService.GetLobbyState();
            await BroadcastMessageAsync(lobbyState);
        }
    }
}
