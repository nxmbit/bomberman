using Bomberman.Server.GameLogic;

namespace Bomberman.Server.GameLogic
{
    public class LobbyService
    {
        private readonly Lobby _lobby = new Lobby();

        public void SetPlayerName(string playerId, string name)
        {
            if (_lobby.Players.TryGetValue(playerId, out var player))
            {
                player.Name = name;
            }
        }
        public void AddPlayer(string playerId, string name)
        {
            if (!_lobby.Players.ContainsKey(playerId))
            {
            var player = new Player(playerId, name);
            _lobby.Players.Add(playerId, player);
            }
        }

        public void SetPlayerReady(string playerId)
        {
            if (_lobby.Players.TryGetValue(playerId, out var player))
            {
                player.IsReady = true;
            }
        }

        public void SetPlayerUnready(string playerId)
        {
            if (_lobby.Players.TryGetValue(playerId, out var player))
            {
                player.IsReady = false;
            }
        }        

        public void RemovePlayer(string playerId)
        {
            _lobby.Players.Remove(playerId);
        }

        public bool AreAllPlayersReady()
        {
            return _lobby.Players.Values.All(p => p.IsReady);
        }

        public bool CanAddPlayer()
        {
            return _lobby.PlayersInLobby < Lobby.MaxPlayers;
        }

        public Dictionary<string, Player> GetPlayers()
        {
            return _lobby.Players;
        }

        public object GetLobbyState()
        {
            return new
            {
                Type = ServerCommandType.SERVER_LOBBY_UPDATE,
                Payload = _lobby.Players.Values.Select(p => new { p.Id, p.Name, p.IsReady })
            };
        }
    }
}