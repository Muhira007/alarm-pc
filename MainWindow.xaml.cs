using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace AlarmGiliran;

public partial class MainWindow : Window
{
    private readonly TurnTimer timer = new();
    private TimerSettings settings = SettingsStore.Load();
    private readonly DispatcherTimer ticker = new() { Interval = TimeSpan.FromMilliseconds(200) };
    private SettingsWindow? settingsWindow;
    private AlarmWindow? alarmWindow;

    public MainWindow()
    {
        InitializeComponent();
        Width = Math.Min(1000, SystemParameters.WorkArea.Width - 24);
        Height = Math.Min(740, SystemParameters.WorkArea.Height - 24);
        timer.Configure(settings.Minutes, settings.FirstPlayer);
        ticker.Tick += (_, _) => UpdateTimer();
        ticker.Start();
        Refresh();
    }

    private void UpdateTimer()
    {
        if (timer.Tick(DateTimeOffset.UtcNow))
        {
            settingsWindow?.Close();
            var previousState = WindowState;
            WindowState = WindowState.Normal;
            Refresh();
            alarmWindow = new AlarmWindow(timer.NextPlayer, timer.Minutes) { Owner = this };
            alarmWindow.ShowDialog();
            alarmWindow = null;
            if (!App.AllowExit)
            {
                timer.Acknowledge(DateTimeOffset.UtcNow);
                WindowState = previousState;
            }
        }
        Refresh();
    }

    private void Refresh()
    {
        bool ready = timer.Phase == TimerPhase.Ready;
        var remaining = timer.Remaining(DateTimeOffset.UtcNow);
        int totalSeconds = (int)Math.Ceiling(remaining.TotalSeconds);
        TimeText.Text = $"{totalSeconds / 60:00}:{totalSeconds % 60:00}";
        PlayerHeading.Text = $"{(ready ? "Giliran pertama" : "Sedang bermain")}: {timer.CurrentPlayer}";
        NextText.Text = $"Berikutnya {timer.NextPlayer} · {timer.Minutes} menit";
        DurationText.Text = $"{timer.Minutes} menit";
        FirstPlayerText.Text = ready ? timer.CurrentPlayer : $"Giliran ke-{timer.TurnNumber}";
        FirstLabel.Text = ready ? "MULAI DARI" : "SESI BERJALAN";
        SwitchPlayerButton.IsEnabled = ready;
        TwentyButton.IsEnabled = ThirtyButton.IsEnabled = ready;
        StartButton.Content = ready ? $"Mulai giliran {timer.CurrentPlayer}  →" : "Kecilkan jendela • alarm tetap berjalan";
        TimerCaption.Text = ready ? "menit bermain" : $"selesai pukul {timer.Deadline.ToLocalTime():HH:mm}";
        StatusText.Text = timer.Phase switch
        {
            TimerPhase.Ready => "SIAP DIMULAI  •  Pilih durasi dan pemain pertama",
            TimerPhase.AwaitingStop => "WAKTUNYA BERGANTI  •  Menunggu tombol Stop",
            _ => "●  ALARM AKTIF  •  Giliran berganti otomatis"
        };
        double fraction = Math.Clamp(remaining.TotalSeconds / timer.ActiveDuration.TotalSeconds, 0, 1);
        if (fraction <= 0) { ProgressArc.Data = null; return; }
        double angle = Math.Min(fraction, 0.99999) * Math.PI * 2;
        const double radius = 115.5;
        var end = new Point(127 + radius * Math.Sin(angle), 127 - radius * Math.Cos(angle));
        var figure = new PathFigure { StartPoint = new Point(127, 11.5), IsClosed = false };
        figure.Segments.Add(new ArcSegment(end, new Size(radius, radius), 0, fraction > 0.5, SweepDirection.Clockwise, true));
        ProgressArc.Data = new PathGeometry(new[] { figure });
    }

    private void StartClick(object sender, RoutedEventArgs e)
    {
        if (timer.Phase == TimerPhase.Ready) timer.Start(DateTimeOffset.UtcNow);
        else WindowState = WindowState.Minimized;
        Refresh();
    }

    private void DurationClick(object sender, RoutedEventArgs e)
    {
        if (timer.Phase != TimerPhase.Ready) return;
        SaveSettings(settings with { Minutes = int.Parse((string)((Button)sender).Tag) });
    }

    private void SwitchPlayerClick(object sender, RoutedEventArgs e)
    {
        if (timer.Phase != TimerPhase.Ready) return;
        SaveSettings(settings with { FirstPlayer = timer.NextPlayer });
    }

    internal bool SaveSettings(TimerSettings updated)
    {
        try { SettingsStore.Save(updated); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(this, "Pengaturan belum dapat disimpan. Periksa izin folder pengguna, lalu coba lagi.", "Pengaturan", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
        settings = updated;
        timer.Configure(settings.Minutes, settings.FirstPlayer);
        Refresh();
        return true;
    }

    private void SettingsClick(object sender, RoutedEventArgs e)
    {
        settingsWindow = new SettingsWindow(settings, timer.Phase != TimerPhase.Ready, SaveSettings) { Owner = this };
        settingsWindow.ShowDialog();
        settingsWindow = null;
    }

    private void MinimizeClick(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void DragHeader(object sender, MouseButtonEventArgs e) { if (e.ChangedButton == MouseButton.Left) DragMove(); }
    private void OnClosing(object? sender, CancelEventArgs e)
    {
        if (!App.AllowExit)
        {
            e.Cancel = true;
            FooterText.Text = "Untuk keluar, buka Pengaturan → Keluar aplikasi.";
        }
        else { ticker.Stop(); AlarmSound.Stop(); }
    }
}
