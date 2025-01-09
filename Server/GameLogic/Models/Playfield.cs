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
        public List<Player> Players { get; set; }
        public Timer Timer { get; set; }

        public enum SpawnPoints
        {
            TopLeft,
            BottomLeft,
            TopRight,
            BottomRight
        }

        public readonly Dictionary<SpawnPoints, (int X, int Y)> SpawnPointLocations;

        //TODO generate map and items
        public Playfield(int width, int height, double blockDensity, List<Player> Players)
        {
            this.Width = width;
            this.Height = height;
            this.Blocks = new List<Block>();
            this.Walls = new List<Wall>();
            this.Bombs = new List<Bomb>();
            this.Explosions = new List<Explosion>();
            this.Items = new List<Item>();
            this.Players = Players;
            this.Timer = new Timer();
            var random = new Random();

            SpawnPointLocations = new Dictionary<SpawnPoints, (int X, int Y)>
            {
                {SpawnPoints.TopLeft, (1, 1)},
                {SpawnPoints.BottomLeft, (1, height - 2)},
                {SpawnPoints.TopRight, (width - 2, 1)},
                {SpawnPoints.BottomRight, (width - 2, height - 2)}
            };

            // randomly select a spawn point for each player
            var shuffledSpawnPoints = Enum.GetValues(typeof(SpawnPoints)).Cast<SpawnPoints>().OrderBy(x => random.Next()).ToList();
            for (int i = 0; i < Players.Count; i++)
            {
                var spawnPoint = shuffledSpawnPoints[i];
                var location = SpawnPointLocations[spawnPoint];
                Players[i].X = location.X;
                Players[i].Y = location.Y;
            }

            // Code for generating the map, the width and height should be odd numbers as
            // the walls are placed on odd coordinates (except of the border). The map has a border of walls.

            //Check if width and height are odd, if not add 1
            if (width % 2 == 0)
            {
                width++;
            }

            if (height % 2 == 0)
            {
                height++;
            }

            //generate border
            for (int x = 0; x < width; x++)
            {
                Walls.Add(new Wall(x, 0));
                Walls.Add(new Wall(x, height - 1));
            }

            for (int y = 0; y < height; y++)
            {
                Walls.Add(new Wall(0, y));
                Walls.Add(new Wall(width - 1, y));
            }

            //generate blocks
            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    if (random.NextDouble() < blockDensity)
                    {
                        Blocks.Add(new Block(x, y));
                    }
                }
            }
            
            //clear player spawn areas
            Blocks.RemoveAll(b => b.X < 3 && b.Y < 3); //top left
            Blocks.RemoveAll(b => b.X < 3 && b.Y > height - 4); //bottom left
            Blocks.RemoveAll(b => b.X > width - 4 && b.Y < 3); //top right
            Blocks.RemoveAll(b => b.X > width - 4 && b.Y > height - 4); //bottom right

            //generate walls
            for (int x = 2; x < width; x += 2)
            {
                for (int y = 2; y < height; y += 2)
                {
                    Walls.Add(new Wall(x, y));
                    //check if a wall is there and delete it
                    Blocks.RemoveAll(b => b.X == x && b.Y == y);
                }
            }
            
            //generate items
            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
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
        }

    }
}