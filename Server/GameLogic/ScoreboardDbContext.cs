using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;


namespace Bomberman.Server.GameLogic
{
    public class ScoreboardDbContext : DbContext
    {
        public DbSet<User> User { get; set; }
        public DbSet<ScoreboardEntry> ScoreboardEntry { get; set; }

        public ScoreboardDbContext(DbContextOptions<ScoreboardDbContext> options)
            : base(options)
        {
        }
    }
}