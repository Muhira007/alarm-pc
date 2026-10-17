using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AlarmGiliran;

internal static class UiChecks
{
    private static int checks;

    [STAThread]
    public static void Main()
    {
        var app = new App();
        app.InitializeComponent();
        string output = Path.GetFullPath("artifacts");
        Directory.CreateDirectory(output);
        var main = new MainWindow();
        Render(main, 1000, 740, Path.Combine(output, "main.png"));
        var blocked = new CancelEventArgs();
        Call(main, "OnClosing", main, blocked);
        Check(blocked.Cancel, "Main window rejects close requests");
        var timer = (TurnTimer)typeof(MainWindow).GetField("timer", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(main)!;
        Click(main, "StartButton");
        Check(timer.Phase == TimerPhase.Running && timer.TurnNumber == 1, "Start button begins countdown");
        Render(main, 1000, 740, Path.Combine(output, "running.png"));
        Render(main, 700, 518, Path.Combine(output, "small-screen.png"));

        TimerSettings? saved = null;
        var settings = new SettingsWindow(new TimerSettings(), true, value => { saved = value; return true; });
        Check(!((ComboBox)settings.FindName("FirstPlayerInput")).IsEnabled, "Player selection locked during active turn");
        ((TextBox)settings.FindName("MinutesInput")).Text = "0";
        Call(settings, "SaveClick", settings, new RoutedEventArgs());
        Check(saved == null && ((TextBlock)settings.FindName("ErrorText")).Text.Length > 0, "Settings reject invalid duration");
        ((TextBox)settings.FindName("MinutesInput")).Text = "20";
        Render(settings, 470, 580, Path.Combine(output, "settings.png"));
        Call(settings, "SaveClick", settings, new RoutedEventArgs());
        Check(saved?.Minutes == 20, "Settings save valid duration");

        var alarm = new AlarmWindow("Lukman", 30);
        Render(alarm, 1280, 720, Path.Combine(output, "alarm-lukman.png"));
        Check(((TextBlock)alarm.FindName("PlayerName")).Text == "Lukman", "Popup shows correct next child");
        blocked = new CancelEventArgs();
        Call(alarm, "OnClosing", alarm, blocked);
        Check(blocked.Cancel, "Popup rejects close requests before Stop");
        Click(alarm, "StopButton");
        blocked = new CancelEventArgs();
        Call(alarm, "OnClosing", alarm, blocked);
        Check(!blocked.Cancel, "Stop permits popup dismissal");
        var imran = new AlarmWindow("Imran", 20);
        Render(imran, 1280, 720, Path.Combine(output, "alarm-imran.png"));
        Check(((TextBlock)imran.FindName("PlayerName")).Text == "Imran", "Popup also supports Imran");
        bool audioStarted = AlarmSound.Start();
        AlarmSound.Stop();
        Check(audioStarted, "Windows accepts looping alarm playback");
        App.AllowExit = true;
        blocked = new CancelEventArgs();
        Call(main, "OnClosing", main, blocked);
        Check(!blocked.Cancel, "Settings exit can close main window");
        main.Close();
        imran.Close();
        app.Shutdown();
        Console.WriteLine($"All {checks} UI component checks passed. Renders: {output}");
    }

    private static void Check(bool condition, string name)
    {
        if (!condition) throw new Exception("FAIL: " + name);
        checks++;
        Console.WriteLine("PASS: " + name);
    }

    private static void Click(Window window, string name) => ((Button)window.FindName(name)).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
    private static void Call(object instance, string method, params object[] arguments) => instance.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)!.Invoke(instance, arguments);

    private static void Render(Window window, int width, int height, string path)
    {
        var root = (FrameworkElement)window.Content;
        root.Measure(new Size(width, height));
        root.Arrange(new Rect(0, 0, width, height));
        root.UpdateLayout();
        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        var visual = new DrawingVisual();
        using (DrawingContext context = visual.RenderOpen()) context.DrawRectangle(window.Background, null, new Rect(0, 0, width, height));
        bitmap.Render(visual);
        bitmap.Render(root);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(path);
        encoder.Save(stream);
    }
}
