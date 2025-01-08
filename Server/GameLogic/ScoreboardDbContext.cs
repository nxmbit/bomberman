using Microsoft.EntityFrameworkCore;

namespace Bomberman.Server.GameLogic
{
    public class ScoreboardDbContext : DbContext
    {
        public DbSet<ScoreboardEntry> Scoreboard { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("ScoreboardDB");
        }
    }
}