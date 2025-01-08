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
        public int Lives = GlobalSettings.LIVES;
        public bool IsInvincible = false;
        public bool IsReady { get; set; }
        public bool IsMoving = false;
        public String PlayerDirection = Direction.DOWN;

        public Player(string id, string name)
        {
            Id = id;
            Name = name;
            X = 0;
            Y = 0;
            IsReady = false;
        }
    }

    
}