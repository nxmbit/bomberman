namespace Bomberman.Server.GameLogic;

public class ScoreboardEntry
{
    public User User { get; set; }
    public int TotalScore { get; set; }
    public int TotalWins { get; set; }
    public int TotalKills { get; set; }
    public int TotalEliminations { get; set; }
    public int TotalDeaths { get; set; }
    public int TotalPickedUpPowerups { get; set; }
    public int TotalMatches { get; set; }
    public int TopScore { get; set; }
    public double StoredKDRatio { get; set; }
}