namespace Bomberman.Server.GameLogic
{
    public class GameParameters
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public double BlockDensity { get; set; }
        public int GameTime { get; set; }
        public int Lives { get; set; }

        public void setParameters(int width, int height, double blockDensity, int gameTime, int lives)
        {
            Width = width;
            Height = height;
            BlockDensity = blockDensity;
            GameTime = gameTime;
            Lives = lives;
        }
    }
}