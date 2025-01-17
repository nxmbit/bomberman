namespace Bomberman.Server.GameLogic
{
    public class Item
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Type { get; set; }

        public Item(int x, int y, string type)
        {
            this.X = x;
            this.Y = y;
            this.Type = type;
        }
    }
}