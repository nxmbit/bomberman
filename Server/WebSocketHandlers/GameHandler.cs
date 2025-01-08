using Bomberman.Server.GameLogic;
using System.Net.WebSockets;
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
            _timer = new System.Timers.Timer(_timerInterval);
            _timer.Elapsed += OnTimerElapsed;
            _timer.AutoReset = true;
            _timer.Enabled = false;
        }

        public async Task HandleAsync(string playerId, string type, string direction, WebSocket socket)
        {
            switch (type)
            {
                case ClientCommandType.CLIENT_GAME_BOMB:
                    // if () {
                    //
                    // } else {
                    //
                    // }
                    await BroadcastGameState();
                    break;

                case ClientCommandType.CLIENT_GAME_MOVE:
                    // if () {
                    //
                    // } else {
                    //
                    // }
                    await BroadcastGameState();
                    break;
            }
        }

        public void startGame(List<Player> Players)
        {
            _gameService.StartGame(Players);
            _timer.Enabled = true;

        }

        private async void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _gameService.Tick();
            await BroadcastGameState();
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
