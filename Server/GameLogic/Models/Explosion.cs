namespace Bomberman.Server.GameLogic
{
    public class Explosion
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Time { get; set; }
        public string OwnerId { get; set; }

        public Explosion(int x, int y, string OwnerId)
        {
            this.X = x;
            this.Y = y;
            this.OwnerId = OwnerId;
            this.Time = GlobalSettings.TICK_RATE*GlobalSettings.EXPLOSION_TIMER;
        }
    }
}