namespace Bomberman.Server.GameLogic;

public class Timer
{
    private int TicksLeft = GlobalSettings.TICK_RATE * GlobalSettings.GAME_TIME;
    public int SecondsLeft { get; set; } = GlobalSettings.GAME_TIME;

    public void Tick()
    {
        TicksLeft--;
        if (TicksLeft % GlobalSettings.TICK_RATE == 0)
        {
            SecondsLeft--;
        }
    }
}