using System.Collections.Generic;

namespace Bomberman.Server.GameLogic
{
    public class Playfield
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public List<Block> Blocks { get; set; }
        public List<Wall> Walls { get; set; }
        public List<Bomb> Bombs { get; set; }
        public List<Explosion> Explosions { get; set; }
        public List<Item> Items { get; set; }
        public Dictionary<string, Player> Players { get; set; }
        public Timer Timer { get; set; }

        //TODO generate map and items
        public Playfield(int width, int height, double blockDensity, Dictionary<string, Player> Players)
        {
            this.Width = width;
            this.Height = height;
            this.Blocks = new List<Block>();
            this.Walls = new List<Wall>();
            this.Bombs = new List<Bomb>();
            this.Explosions = new List<Explosion>();
            this.Items = new List<Item>();
            this.Players = Players;
            this.Timer = new Timer(); //TODO: implemt timer
            
            Random random = new Random();
            //generate blocks
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (random.NextDouble() < blockDensity)
                    {
                        Blocks.Add(new Block(x, y));
                    }
                }
            }
            
            //clear player spawn areas
            Blocks.RemoveAll(b => b.X < 2 && b.Y < 2);
            Blocks.RemoveAll(b => b.X < 2 && b.Y > height - 3);
            Blocks.RemoveAll(b => b.X > width - 3 && b.Y < 2);
            Blocks.RemoveAll(b => b.X > width - 3 && b.Y > height - 3);

            //generate walls
            for (int x = 1; x < width; x += 2)
            {
                for (int y = 1; y < height; y += 2)
                {
                    Walls.Add(new Wall(x, y));
                    //check if a wall is there and delete it
                    Blocks.RemoveAll(b => b.X == x && b.Y == y);
                }
            }
            
            //generate items
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (random.NextDouble() < 0.1)
                    {
                        //if there is a block create an item
                        if (Blocks.Any(b => b.X == x && b.Y == y))
                        {
                            Items.Add(new Item(x, y, (ItemType)random.Next(0, 3)));
                        }
                    }
                }
            }
            
            DisplayPlayfield();
        }
        public void DisplayPlayfield()
        {
            char[,] display = new char[Height, Width];

            // Initialize the display with empty spaces
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    display[y, x] = ' ';
                }
            }

            // Place blocks
            foreach (var block in Blocks)
            {
                display[block.Y, block.X] = 'B';
            }

            // Place walls
            foreach (var wall in Walls)
            {
                display[wall.Y, wall.X] = 'W';
            }

            // Place items
            foreach (var item in Items)
            {
                display[item.Y, item.X] = 'I';
            }

            // Display the playfield
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Console.Write(display[y, x]);
                }
                Console.WriteLine();
            }
        }
    }
}