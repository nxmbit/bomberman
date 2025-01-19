namespace Bomberman.Server.GameLogic
{
    public static class Direction
    {
        public const string UP = "up";
        public const string DOWN = "down";
        public const string LEFT = "left";
        public const string RIGHT = "right";
    }
    
    public class Player
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public int Lives { get; set; }
        public bool IsReady { get; set; }
        public int Score {get; set;}
        public int BombLimit { get; set; }
        public int BombPower { get; set; }
        public double Speed { get; set; }
        public bool IsInvincible { get; set; }
        public int InvincibilityTicks { get; set; }
        public string Color { get; set; }
        public bool IsMoving = false;
        public string PlayerDirection = Direction.DOWN;


        public Player(string id, string name, string color)
        {
            Id = id;
            Name = name;
            Color = color;
            Lives = GlobalSettings.LIVES;
            X = 0;
            Y = 0;
            BombLimit = GlobalSettings.INITIAL_BOMB_LIMIT;
            Score = 0;
            IsReady = false;
            IsInvincible = false;
        }
    }

    
}