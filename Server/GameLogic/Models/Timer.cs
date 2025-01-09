namespace Bomberman.Server.GameLogic;

public class Timer
{
    private int TicksLeft;
    public int SecondsLeft { get; set; }

    public Timer(int seconds)
    {
        SecondsLeft = seconds;
        TicksLeft = seconds * GlobalSettings.TICK_RATE;
    }

    public void Tick()
    {
        TicksLeft--;
        if (TicksLeft % GlobalSettings.TICK_RATE == 0)
        {
            SecondsLeft--;
        }
    }
}