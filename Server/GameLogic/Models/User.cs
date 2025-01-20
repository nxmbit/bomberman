namespace Bomberman.Server.GameLogic;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }

    private void SetPassword(string password)
    {
        PasswordHash = new PasswordService().HashPassword(this, password);
    }

    public User() {}

    public User(string username, string password)
    {
        Username = username;
        SetPassword(password);
    }
}