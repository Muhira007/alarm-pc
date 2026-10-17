using System.Windows;
using System.Windows.Controls;

namespace AlarmGiliran;

public partial class SettingsWindow : Window
{
    private readonly Func<TimerSettings, bool> save;

    public SettingsWindow(TimerSettings settings, bool running, Func<TimerSettings, bool> saveSettings)
    {
        InitializeComponent();
        save = saveSettings;
        MinutesInput.Text = settings.Minutes.ToString();
        FirstPlayerInput.SelectedIndex = settings.FirstPlayer == "Imran" ? 0 : 1;
        FirstPlayerInput.IsEnabled = !running;
        if (running) SettingsNote.Text = "Durasi baru berlaku mulai giliran berikutnya. Waktu yang sedang berjalan tetap utuh.";
    }

    private void BackClick(object sender, RoutedEventArgs e) => Close();
    private void SaveClick(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(MinutesInput.Text, out int minutes) || minutes is < 1 or > 180)
        {
            ErrorText.Text = "Isi durasi dengan angka bulat antara 1 dan 180 menit.";
            MinutesInput.Focus();
            return;
        }
        string player = (string)((ComboBoxItem)FirstPlayerInput.SelectedItem).Content;
        if (save(new TimerSettings(minutes, player))) Close();
    }

    private void ExitClick(object sender, RoutedEventArgs e)
    {
        App.AllowExit = true;
        Application.Current.Shutdown();
    }
}
