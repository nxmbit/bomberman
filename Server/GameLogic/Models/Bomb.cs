namespace Bomberman.Server.GameLogic
{
    public class Bomb
    {
        public string OwnerId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Range { get; set; } = 2;
        
        public int Fuse { get; set; } = GlobalSettings.BOMB_TIMER * GlobalSettings.TICK_RATE;

        public Bomb(string ownerId, int x, int y)
        {
            OwnerId = ownerId;
            X = x;
            Y = y;
        }
    }
}
