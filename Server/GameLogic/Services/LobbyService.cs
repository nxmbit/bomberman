using Bomberman.Server.GameLogic;

namespace Bomberman.Server.GameLogic
{
    public class LobbyService
    {
        private readonly Lobby _lobby = new Lobby();
        private GameParameters _gameParameters = new GameParameters();

        private const int DEFAULT_WIDTH = 15;
        private const int DEFAULT_HEIGHT = 15;
        private const double DEFAULT_BLOCK_DENSITY = 0.5;
        private const int DEFAULT_GAME_TIME = 180;
        private const int DEFAULT_LIVES = 3;
        private const int DEFAULT_START_POWER = 1;
        private const int DEFAULT_START_BOMBS = 1;
        private const int DEFAULT_START_SPEED = 1;


        public LobbyService()
        {
            _gameParameters.setParameters(DEFAULT_WIDTH, DEFAULT_HEIGHT, DEFAULT_BLOCK_DENSITY, DEFAULT_GAME_TIME,
                DEFAULT_LIVES, DEFAULT_START_POWER, DEFAULT_START_BOMBS, DEFAULT_START_SPEED);
        }

        public void SetGameParameters(int width, int height, double blockDensity, int gameTime, int lives, int startPower,
            int bombRange, double startSpeed)
        {
            _gameParameters.setParameters(width, height, blockDensity, gameTime, lives, startPower, bombRange, startSpeed);
        }

        public GameParameters GetGameParameters()
        {
            return _gameParameters;
        }

        public void SetPlayerName(string playerId, string name)
        {
            var player = _lobby.Players.FirstOrDefault(p => p.Id == playerId);
            if (player != null)
            {
                player.Name = name;
            }
        }

        public void AddPlayer(string playerId, string name)
        {
            if (_lobby.Players.TrueForAll(p => p.Id != playerId) && _lobby.MaxPlayers > _lobby.PlayersInLobby)
            {
                _lobby.Players.Add(new Player(playerId, name));
            }
        }

        public void SetPlayerReady(string playerId)
        {
            var player = _lobby.Players.FirstOrDefault(p => p.Id == playerId);
            if (player != null)
            {
                player.IsReady = true;
            }
        }

        public void SetPlayerUnready(string playerId)
        {
            var player = _lobby.Players.FirstOrDefault(p => p.Id == playerId);
            if (player != null)
            {
                player.IsReady = false;
            }
        }

        public void RemovePlayer(string playerId)
        {
            _lobby.Players.RemoveAll(p => p.Id == playerId);
        }

        public bool AreAllPlayersReady()
        {
            return _lobby.Players.TrueForAll(p => p.IsReady);
        }

        public bool CanAddPlayer()
        {
            return _lobby.PlayersInLobby < _lobby.MaxPlayers;
        }

        public List<Player> GetPlayers()
        {
            return _lobby.Players;
        }

        private void ResetGameParameters()
        {
            _gameParameters.setParameters(DEFAULT_WIDTH, DEFAULT_HEIGHT, DEFAULT_BLOCK_DENSITY, DEFAULT_GAME_TIME,
                DEFAULT_LIVES, DEFAULT_START_POWER, DEFAULT_START_BOMBS, DEFAULT_START_SPEED);
        }

        public void resetLobby()
        {
            _lobby.Players.Clear();
            ResetGameParameters();
        }

        public object GetLobbyState()
        {
            return new
            {
                Type = ServerCommandType.SERVER_LOBBY_UPDATE,
                Payload = _lobby.Players.Select(p => new { p.Id, p.Name, p.IsReady })
            };
        }

        public object GetLobbySettings()
        {
            return new
            {
                Type = ServerCommandType.SERVER_LOBBY_UPDATE_SETTINGS,
                Payload = _gameParameters
            };
        }
    }
}