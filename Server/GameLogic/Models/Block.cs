namespace Bomberman.Server.GameLogic
{
    public class Block
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Block(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}