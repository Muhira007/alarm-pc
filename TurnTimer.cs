namespace AlarmGiliran;

public enum TimerPhase { Ready, Running, AwaitingStop }

// Pure timer logic; the deadline includes time spent sleeping or with a busy UI.
public sealed class TurnTimer
{
    public TimerPhase Phase { get; private set; }
    public string CurrentPlayer { get; private set; } = "Imran";
    public string NextPlayer => CurrentPlayer == "Imran" ? "Lukman" : "Imran";
    public int Minutes { get; private set; } = 30;
    public TimeSpan ActiveDuration { get; private set; } = TimeSpan.FromMinutes(30);
    public DateTimeOffset Deadline { get; private set; }
    public int TurnNumber { get; private set; }

    public void Configure(int minutes, string firstPlayer)
    {
        if (minutes is < 1 or > 180) throw new ArgumentOutOfRangeException(nameof(minutes));
        if (firstPlayer is not ("Imran" or "Lukman")) throw new ArgumentException("Nama tidak valid.", nameof(firstPlayer));
        Minutes = minutes;
        if (Phase == TimerPhase.Ready)
        {
            CurrentPlayer = firstPlayer;
            ActiveDuration = TimeSpan.FromMinutes(minutes);
        }
    }

    public void Start(DateTimeOffset now)
    {
        if (Phase != TimerPhase.Ready) return;
        TurnNumber = 1;
        BeginTurn(now);
    }

    public bool Tick(DateTimeOffset now)
    {
        if (Phase != TimerPhase.Running || now < Deadline) return false;
        Phase = TimerPhase.AwaitingStop;
        return true;
    }

    public void Acknowledge(DateTimeOffset now)
    {
        if (Phase != TimerPhase.AwaitingStop) return;
        CurrentPlayer = NextPlayer;
        TurnNumber++;
        BeginTurn(now);
    }

    public TimeSpan Remaining(DateTimeOffset now) => Phase switch
    {
        TimerPhase.Ready => TimeSpan.FromMinutes(Minutes),
        TimerPhase.AwaitingStop => TimeSpan.Zero,
        _ => Deadline > now ? Deadline - now : TimeSpan.Zero
    };

    private void BeginTurn(DateTimeOffset now)
    {
        ActiveDuration = TimeSpan.FromMinutes(Minutes);
        Deadline = now + ActiveDuration;
        Phase = TimerPhase.Running;
    }
}
