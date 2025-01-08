namespace Bomberman.Server.GameLogic
{
    public class Explosion
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Time = 3 * GlobalSettings.TICK_RATE;
        public string PlayerId { get; set; }

        public Explosion(int x, int y, string playerId)
        {
            this.X = x;
            this.Y = y;
            this.PlayerId = playerId;
        }
    }
}