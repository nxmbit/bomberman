using Bomberman.Server.GameLogic;
using System.Net.WebSockets;
using System.Text.Json;

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
                        foreach (var sock in _sockets.Keys)
                        {
                            await SendMessageAsync(sock, new { Type = ServerCommandType.SERVER_GAME_START });
                        }
                        var playersCopy = new List<Player>(_lobbyService.GetPlayers());
                        Console.WriteLine(_lobbyService.GetPlayers());
                        _gameHandler.startGame(playersCopy, _lobbyService.GetGameParameters());
                        _lobbyService.resetLobby();
                    }
                    break;

                case ClientCommandType.CLIENT_LOBBY_UPDATE_SETTINGS:
                    Dictionary<string, dynamic> data = JsonSerializer.Deserialize<Dictionary<string, dynamic>>(payload);

                    int width = _lobbyService.GetGameParameters().Width;
                    int height = _lobbyService.GetGameParameters().Height;
                    double blockDensity = _lobbyService.GetGameParameters().BlockDensity;
                    int gameTime = _lobbyService.GetGameParameters().GameTime;
                    int lives = _lobbyService.GetGameParameters().Lives;

                    if (data.TryGetValue("Width", out var widthValue)) width = (int)widthValue;
                    if (data.TryGetValue("Height", out var heightValue)) height = (int)heightValue;
                    if (data.TryGetValue("BlockDensity", out var blockDensityValue)) blockDensity = (double)blockDensityValue;
                    if (data.TryGetValue("GameTime", out var gameTimeValue)) gameTime = (int)gameTimeValue;
                    if (data.TryGetValue("Lives", out var livesValue)) lives = (int)livesValue;

                    _lobbyService.SetGameParameters(width, height, blockDensity, gameTime, lives);
                    await BroadcastLobbySettings();
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

        private async Task BroadcastLobbySettings()
        {
            var lobbySettings = _lobbyService.GetLobbySettings();
            await BroadcastMessageAsync(lobbySettings);
        }
    }
}
