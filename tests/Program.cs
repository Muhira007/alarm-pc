using AlarmGiliran;

int checks = 0;
void Check(bool condition, string name)
{
    if (!condition) throw new Exception("FAIL: " + name);
    checks++;
    Console.WriteLine("PASS: " + name);
}
var now = DateTimeOffset.Parse("2026-10-17T00:00:00Z");
var timer = new TurnTimer();
timer.Configure(30, "Imran");
Check(timer.Remaining(now) == TimeSpan.FromMinutes(30), "Initial duration");
timer.Start(now);
Check(timer.CurrentPlayer == "Imran" && timer.TurnNumber == 1, "First player starts");
Check(!timer.Tick(now.AddMinutes(30).AddMilliseconds(-1)), "No early alarm");
Check(timer.Tick(now.AddMinutes(30)), "Alarm at exact deadline");
Check(!timer.Tick(now.AddMinutes(31)), "Alarm only fires once");
Check(timer.Phase == TimerPhase.AwaitingStop && timer.NextPlayer == "Lukman", "Wait for Stop with next name");
timer.Acknowledge(now.AddMinutes(35));
Check(timer.CurrentPlayer == "Lukman" && timer.Remaining(now.AddMinutes(35)) == TimeSpan.FromMinutes(30), "Stop gives next child full duration");
timer.Acknowledge(now.AddMinutes(36));
Check(timer.CurrentPlayer == "Lukman" && timer.TurnNumber == 2, "Duplicate Stop cannot skip turn");
timer.Configure(20, "Imran");
Check(timer.Deadline == now.AddMinutes(65) && timer.CurrentPlayer == "Lukman", "Settings preserve active turn");
Check(timer.Tick(now.AddHours(4)), "Sleep past deadline triggers alarm");
timer.Acknowledge(now.AddHours(4));
Check(timer.CurrentPlayer == "Imran" && timer.Remaining(now.AddHours(4)) == TimeSpan.FromMinutes(20), "New duration used next turn without skipping children");
for (int i = 0; i < 20; i++)
{
    var deadline = timer.Deadline;
    string expected = timer.NextPlayer;
    timer.Tick(deadline);
    timer.Acknowledge(deadline);
    Check(timer.CurrentPlayer == expected && timer.Remaining(deadline) == TimeSpan.FromMinutes(20), $"Repeated alternation {i + 1}");
}
var second = new TurnTimer();
second.Configure(20, "Lukman");
second.Start(now);
Check(second.CurrentPlayer == "Lukman" && second.NextPlayer == "Imran", "Lukman can start first");
foreach (int invalid in new[] { 0, -1, 181 })
{
    bool rejected = false;
    try { second.Configure(invalid, "Imran"); } catch (ArgumentOutOfRangeException) { rejected = true; }
    Check(rejected, $"Reject duration {invalid}");
}
Console.WriteLine($"All {checks} checks passed.");
