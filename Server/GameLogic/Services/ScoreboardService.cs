namespace Bomberman.Server.GameLogic;

public class ScoreboardService
{
    private readonly ScoreboardDbContext _context;

    public ScoreboardService(ScoreboardDbContext context)
    {
        _context = context;
    }

    public void AddOrUpdateScoreboardEntry(User user, int score, int wins, int kills, int eliminations, int deaths, int pickedUpPowerups)
    {
        var entry = _context.ScoreboardEntry.FirstOrDefault(e => e.User.Id == user.Id);
        if (entry == null)
        {
            entry = new ScoreboardEntry
            {
                User = user,
                TotalScore = score,
                TotalWins = wins,
                TotalKills = kills,
                TotalEliminations = eliminations,
                TotalDeaths = deaths,
                TotalPickedUpPowerups = pickedUpPowerups,
                StoredKDRatio = deaths == 0 ? kills : (double)kills / deaths
            };
            _context.ScoreboardEntry.Add(entry);
        }
        else
        {
            entry.TotalScore += score;
            entry.TotalWins += wins;
            entry.TotalKills += kills;
            entry.TotalEliminations += eliminations;
            entry.TotalDeaths += deaths;
            entry.TotalPickedUpPowerups += pickedUpPowerups;
            entry.StoredKDRatio = entry.TotalDeaths == 0 ? entry.TotalKills : (double)entry.TotalKills / entry.TotalDeaths;
        }

        _context.SaveChanges();
    }

    public List<ScoreboardEntry> GetScoreboard()
    {
        return _context.ScoreboardEntry.OrderByDescending(e => e.TotalScore).ToList();
    }
}