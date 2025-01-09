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

        private enum SpawnPoints
        {
            TopLeft,
            BottomLeft,
            TopRight,
            BottomRight
        }

        private readonly Dictionary<SpawnPoints, (int X, int Y)> SpawnPointLocations;

        //TODO generate map and items
        public Playfield(GameParameters gameParameters, List<Player> Players)
        {
            this.Width = gameParameters.Width;
            this.Height = gameParameters.Height;
            this.Blocks = new List<Block>();
            this.Walls = new List<Wall>();
            this.Bombs = new List<Bomb>();
            this.Explosions = new List<Explosion>();
            this.Items = new List<Item>();
            this.Players = Players;
            this.Timer = new Timer(gameParameters.GameTime);
            var random = new Random();

            SpawnPointLocations = new Dictionary<SpawnPoints, (int X, int Y)>
            {
                {SpawnPoints.TopLeft, (1, 1)},
                {SpawnPoints.BottomLeft, (1, this.Height - 2)},
                {SpawnPoints.TopRight, (this.Width - 2, 1)},
                {SpawnPoints.BottomRight, (this.Width - 2, this.Height - 2)}
            };

            // set lives for each player
            foreach (var player in Players)
            {
                player.Lives = gameParameters.Lives;
            }

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
            if (this.Width % 2 == 0)
            {
                this.Width++;
            }

            if (this.Height % 2 == 0)
            {
                this.Height++;
            }

            //generate border
            for (int x = 0; x < this.Width; x++)
            {
                Walls.Add(new Wall(x, 0));
                Walls.Add(new Wall(x, this.Height - 1));
            }

            for (int y = 0; y < this.Height; y++)
            {
                Walls.Add(new Wall(0, y));
                Walls.Add(new Wall(this.Width - 1, y));
            }

            //generate blocks
            for (int x = 1; x < this.Width - 1; x++)
            {
                for (int y = 1; y < this.Height - 1; y++)
                {
                    if (random.NextDouble() < gameParameters.BlockDensity)
                    {
                        Blocks.Add(new Block(x, y));
                    }
                }
            }
            
            //clear player spawn areas
            Blocks.RemoveAll(b => b.X < 3 && b.Y < 3); //top left
            Blocks.RemoveAll(b => b.X < 3 && b.Y > this.Height - 4); //bottom left
            Blocks.RemoveAll(b => b.X > this.Width - 4 && b.Y < 3); //top right
            Blocks.RemoveAll(b => b.X > this.Width - 4 && b.Y > this.Height - 4); //bottom right

            //generate walls
            for (int x = 2; x < this.Width; x += 2)
            {
                for (int y = 2; y < this.Height; y += 2)
                {
                    Walls.Add(new Wall(x, y));
                    //check if a wall is there and delete it
                    Blocks.RemoveAll(b => b.X == x && b.Y == y);
                }
            }
            
            //generate items
            for (int x = 1; x < this.Width - 1; x++)
            {
                for (int y = 1; y < this.Height - 1; y++)
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