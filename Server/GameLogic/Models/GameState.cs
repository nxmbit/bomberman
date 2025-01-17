using System.Collections.Concurrent;

namespace Bomberman.Server.GameLogic
{
    public class GameState
    {
        // this holds the map and the timer for the purpouse of sending game state to the clients
        public Playfield Playfield { get; set; }
    }
}
