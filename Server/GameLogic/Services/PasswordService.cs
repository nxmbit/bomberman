using Microsoft.AspNetCore.Identity;

namespace Bomberman.Server.GameLogic;

public class PasswordService
{
    private readonly PasswordHasher<User> _passwordHasher = new PasswordHasher<User>();

    public string HashPassword(User user, string password)
    {
        return _passwordHasher.HashPassword(user, password);
    }

    public bool VerifyHashedPassword(User user, string hashedPassword, string providedPassword)
    {
        return _passwordHasher.VerifyHashedPassword(user, hashedPassword, providedPassword) == PasswordVerificationResult.Success;
    }

}