namespace Bomberman.Server.GameLogic
{
    public class Wall
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Wall(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}