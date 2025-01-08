namespace Bomberman.Server.GameLogic
{
    public class Lobby
    {
        public List<Player> Players = new List<Player>();
        public const int MaxPlayers = 4;
        public int PlayersInLobby => Players.Count;

    }
}