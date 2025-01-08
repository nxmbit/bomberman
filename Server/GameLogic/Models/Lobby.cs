namespace Bomberman.Server.GameLogic
{
    public class Lobby {
        public Dictionary<string, Player> Players { get; set; } = new Dictionary<string, Player>();
        public const int MaxPlayers = 4;
        public int PlayersInLobby => Players.Count;

    }
}