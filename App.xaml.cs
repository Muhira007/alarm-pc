using System.Threading;
using System.Windows;

namespace AlarmGiliran;

public partial class App : Application
{
    private Mutex? instanceMutex;
    public static bool AllowExit { get; set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        instanceMutex = new Mutex(true, @"Local\AlarmGiliran.ImranLukman", out bool created);
        if (!created)
        {
            MessageBox.Show("Alarm Giliran sudah berjalan. Buka melalui taskbar Windows.", "Alarm Giliran", MessageBoxButton.OK, MessageBoxImage.Information);
            Shutdown();
            return;
        }
        SessionEnding += (_, _) => AllowExit = true;
        MainWindow = new MainWindow();
        MainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        AlarmSound.Stop();
        instanceMutex?.Dispose();
        base.OnExit(e);
    }
}
