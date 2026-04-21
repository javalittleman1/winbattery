using System.Text.Json;

namespace WinBattery.Core;

public class AppConfig
{
    public string Language { get; set; } = "system";
    public string ThemeMode { get; set; } = "system";
    public bool AutoStart { get; set; } = false;
    public bool FloatWindow { get; set; } = false;
    public int RefreshInterval { get; set; } = 5;
}

public static class ConfigService
{
    private static readonly string ConfigPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WinBattery", "config.json");

    public static AppConfig Config { get; private set; } = new();

    public static void Load()
    {
        try
        {
            if (File.Exists(ConfigPath))
            {
                var json = File.ReadAllText(ConfigPath);
                var cfg = JsonSerializer.Deserialize<AppConfig>(json);
                if (cfg != null) Config = cfg;
            }
        }
        catch { }

        // 应用配置
        var lang = Config.Language == "system" ? I18nService.DetectSystemLanguage() : Config.Language;
        I18nService.SetLanguage(lang);
        ThemeService.SetMode(Config.ThemeMode);
    }

    public static void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);
            var json = JsonSerializer.Serialize(Config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigPath, json);
        }
        catch { }
    }
}
