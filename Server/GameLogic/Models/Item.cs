namespace Bomberman.Server.GameLogic
{
    public class Item
    {
        public int X { get; set; }
        public int Y { get; set; }
        private ItemType Type { get; set; }

        public Item(int x, int y, ItemType type)
        {
            this.X = x;
            this.Y = y;
            this.Type = type;
        }
    }
}