using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Bomberman.Server.GameLogic;
using Bomberman.Server.Services;

namespace Bomberman.Server.WebSocketHandlers
{
    public class MainWebSocketHandler : BaseHandler
    {
        private readonly GameHandler _gameHandler;
        private readonly LobbyHandler _lobbyHandler;
        private readonly LobbyService _lobbyService;
        private readonly UserService _userService;
        private readonly PasswordService _passwordService;

        public MainWebSocketHandler(LobbyService lobbyService, GameHandler gameHandler, LobbyHandler lobbyHandler,
            UserService userService, PasswordService passwordService)
        {
            _lobbyService = lobbyService;
            _gameHandler = gameHandler;
            _lobbyHandler = lobbyHandler;
            _userService = userService;
            _passwordService = passwordService;
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

            User? user = null;
            var playerId = Guid.NewGuid().ToString();
            Console.WriteLine($"Player {playerId} attempting to connect");
            var buffer = new byte[1024 * 4];

            var result = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);
            var msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
            var command = JsonSerializer.Deserialize<CommandMessage>(msg);

            // check if player is attempting to log in with a username and password, if so, authenticate
            if (command.Type == ClientCommandType.CLIENT_LOBBY_JOIN)
            {
                Dictionary<string, dynamic> data = JsonSerializer.Deserialize<Dictionary<string, dynamic>>(command.Payload);
                string? username = data["Username"].ToString();
                string? password = data["Password"].ToString();

                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    user = await AuthenticateOrRegisterUserAsync(username, password);
                    if (user == null)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.PolicyViolation, "Invalid login credentials", CancellationToken.None);
                        Console.WriteLine("Connection attempt rejected: Invalid login credentials");
                        return;
                    }
                }
            }

            _sockets.TryAdd(webSocket, playerId);

            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    if (result.MessageType == WebSocketMessageType.Close)
                        break;

                    msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    command = JsonSerializer.Deserialize<CommandMessage>(msg);

                    switch (command.Type)
                    {
                        case string type when type.StartsWith("CLIENT_LOBBY"):
                            await _lobbyHandler.HandleAsync(playerId, command.Type, command.Payload, webSocket, user);
                            break;
                        case string type when type.StartsWith("CLIENT_GAME"):
                            await _gameHandler.HandleAsync(playerId, command.Type, command.Payload, webSocket);
                            break;
                    }

                    result = await webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer), CancellationToken.None);
                }
            }
            catch (WebSocketException ex) when (ex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
            {
                Console.WriteLine("WebSocketException: The remote party closed the WebSocket connection without completing the close handshake");
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to receive message from client: " + e);
            }
            finally
            {
                if (_gameHandler.IsGameStarted())
                {
                    // remove player from game
                    _gameHandler.RemovePlayer(playerId);
                    _sockets.TryRemove(webSocket, out _); // remove WebSocket from collection

                } else {
                    // remove player from lobby
                    _lobbyService.RemovePlayer(playerId);
                    // broadcast updated lobby state
                    var lobbyState = _lobbyService.GetLobbyState();
                    _sockets.TryRemove(webSocket, out _); // remove WebSocket from collection

                    if (_lobbyService.GetPlayers().Count == 0)
                    {
                        _lobbyService.resetLobby();
                    } else {
                        await BroadcastMessageAsync(lobbyState);
                    }

                }

                Console.WriteLine($"Player {playerId} disconnected");
            }
        }

        private async Task<User> AuthenticateOrRegisterUserAsync(string username, string password)
        {
            var user = await _userService.GetUserByUsernameAsync(username);
            if (user != null)
            {
                if (_passwordService.VerifyHashedPassword(user, user.PasswordHash, password))
                {
                    return user;
                }
                else
                {
                    return null; // Invalid password
                }
            }
            else
            {
                user = new User(username, password);
                await _userService.AddUserAsync(user);
                return user;
            }
        }

        private class CommandMessage
        {
            public string Type { get; set; }
            public JsonElement Payload { get; set; }
        }
    }
}