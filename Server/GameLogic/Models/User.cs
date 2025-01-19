namespace Bomberman.Server.GameLogic;

public class User
{
    public int Id { get; set; }
    public string UserName { get; set; }
    public string PasswordHash { get; set; }

    public void SetPassword(string password)
    {
        PasswordHash = new PasswordService().HashPassword(this, password);
    }
}