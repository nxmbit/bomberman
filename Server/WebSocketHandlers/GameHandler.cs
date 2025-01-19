using Bomberman.Server.GameLogic;
using System.Net.WebSockets;
using System.Text.Json;
using System.Timers;

namespace Bomberman.Server.WebSocketHandlers
{
    public class GameHandler : BaseHandler
    {
        private readonly GameService _gameService;
        private readonly double _timerInterval = 1000.0 / GlobalSettings.TICK_RATE;
        private readonly System.Timers.Timer _timer;


        public GameHandler(GameService gameService)
        {
            _gameService = gameService;
            _gameService.GameOver += OnGameOver;
            _timer = new System.Timers.Timer(_timerInterval);
            _timer.Elapsed += OnTimerElapsed;
            _timer.AutoReset = true;
            _timer.Enabled = false;
        }

        public void RemovePlayer(string playerId)
        {
            _gameService.removePlayer(playerId);
        }

        public bool IsGameStarted()
        {
            return _gameService.isGameRunning;
        }

        public async Task HandleAsync(string playerId, string type, JsonElement payload, WebSocket socket)
        {
            switch (type)
            {
                case ClientCommandType.CLIENT_GAME_BOMB:
                    _gameService.PlaceBomb(playerId);
                    await BroadcastGameState();
                    break;

                case ClientCommandType.CLIENT_GAME_MOVE:
                    //read payload as json
                    Dictionary<string, dynamic> data = JsonSerializer.Deserialize<Dictionary<string, dynamic>>(payload);
                    _gameService.MovePlayer(playerId, data["Direction"].ToString(),data["KeyDown"].ToString()=="True");
                    await BroadcastGameState();
                    break;
            }
        }

        public void startGame(List<Player> Players, GameParameters gameParameters)
        {
            _gameService.StartGame(Players, gameParameters);
            _timer.Enabled = true;
        }

        private async void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _gameService.Tick();
            await BroadcastGameState();
        }

        private async void OnGameOver(string outcome, string? winnerName, bool isDraw)
        {
            _timer.Enabled = false;
            var msg = new
            {
                Type = ServerCommandType.SERVER_GAME_OVER,
                Payload = new
                {
                    Outcome = outcome,
                    Draw = isDraw,
                    Winner = isDraw ? null : winnerName
                }
            };
            await BroadcastMessageAsync(msg);
            Console.WriteLine($"Game over: {outcome}, Winner: {winnerName}");
        }

        private async Task BroadcastGameState()
        {
            var msg = new
            {
                Type = ServerCommandType.SERVER_GAME_UPDATE,
                Payload = _gameService.GetGameState()
            };
            await BroadcastMessageAsync(msg);
        }
    }
}
