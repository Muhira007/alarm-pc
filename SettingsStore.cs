using System.IO;
using System.Text.Json;

namespace AlarmGiliran;

public sealed record TimerSettings(int Minutes = 30, string FirstPlayer = "Imran");

public static class SettingsStore
{
    private static readonly string SettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AlarmGiliran", "settings.json");

    public static TimerSettings Load()
    {
        try
        {
            var value = JsonSerializer.Deserialize<TimerSettings>(File.ReadAllText(SettingsPath));
            if (value is { Minutes: >= 1 and <= 180, FirstPlayer: "Imran" or "Lukman" }) return value;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException) { }
        return new();
    }

    public static void Save(TimerSettings settings)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
        string temporaryPath = SettingsPath + ".tmp";
        File.WriteAllText(temporaryPath, JsonSerializer.Serialize(settings));
        File.Move(temporaryPath, SettingsPath, true);
    }
}
