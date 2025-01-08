namespace Bomberman.Server.GameLogic
{
    public class Player
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public int Lives = GlobalSettings.LIVES;
        public bool IsInvincible = false;
        public bool IsReady { get; set; }

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
