namespace Bomberman.Server.GameLogic;

public class ScoreboardService
{
    private readonly IServiceProvider _serviceProvider;

    public ScoreboardService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void AddOrUpdateScoreboardEntry(User user, int score, bool wins, int kills, int eliminations, int deaths, int pickedUpPowerups)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var _context = scope.ServiceProvider.GetRequiredService<ScoreboardDbContext>();
            var entry = _context.ScoreboardEntry.FirstOrDefault(e => e.User.Id == user.Id);

            if (entry != null)
            {
                entry.TotalScore += score;
                entry.TotalWins += wins ? 1 : 0;
                entry.TotalKills += kills;
                entry.TotalEliminations += eliminations;
                entry.TotalDeaths += deaths;
                entry.TotalPickedUpPowerups += pickedUpPowerups;
                entry.StoredKDRatio = entry.TotalDeaths == 0
                    ? entry.TotalKills
                    : (double)entry.TotalKills / entry.TotalDeaths;
                entry.TotalMatches++;
                entry.TopScore = Math.Max(entry.TopScore, score);
            }

            _context.SaveChanges();
        }
    }

    // public List<ScoreboardEntry> GetScoreboard()
    // {
    //     return _context.ScoreboardEntry.OrderByDescending(e => e.TotalScore).ToList();
    // }
}