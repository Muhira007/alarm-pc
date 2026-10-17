using System.ComponentModel;
using System.Windows;

namespace AlarmGiliran;

public partial class AlarmWindow : Window
{
    private bool acknowledged;

    public AlarmWindow(string nextPlayer, int minutes)
    {
        InitializeComponent();
        PlayerName.Text = nextPlayer;
        DurationHint.Text = $"Serahkan laptop, lalu mulai {minutes} menit berikutnya.";
        Loaded += (_, _) =>
        {
            Activate();
            StopButton.Focus();
            if (!AlarmSound.Start()) StopHint.Text = "Suara tidak tersedia. Tekan Stop untuk memulai giliran.";
        };
        Closed += (_, _) => AlarmSound.Stop();
    }

    private void StopClick(object sender, RoutedEventArgs e)
    {
        acknowledged = true;
        Close();
    }

    private void OnClosing(object? sender, CancelEventArgs e) => e.Cancel = !acknowledged && !App.AllowExit;
}
