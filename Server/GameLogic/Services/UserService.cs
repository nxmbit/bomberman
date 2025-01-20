using System.Threading.Tasks;
using Bomberman.Server.GameLogic;
using Microsoft.EntityFrameworkCore;

namespace Bomberman.Server.Services
{
    public class UserService
    {
        private readonly ScoreboardDbContext _context;

        public UserService(ScoreboardDbContext context)
        {
            _context = context;
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await _context.User.SingleOrDefaultAsync(u => u.Username == username);
        }

        public async Task AddUserAsync(User user)
        {
            _context.User.Add(user);
            _context.ScoreboardEntry.Add(new ScoreboardEntry
            {
                User = user
            });
            await _context.SaveChangesAsync();
        }
    }
}