using Microsoft.Win32;

namespace WinBattery.Core;

public class ThemeColors
{
    public Color Bg { get; set; }
    public Color Sidebar { get; set; }
    public Color Card { get; set; }
    public Color Text { get; set; }
    public Color Text2 { get; set; }
    public Color Border { get; set; }
    public Color Accent { get; set; }
    public Color Success { get; set; }
    public Color Warning { get; set; }
    public Color Danger { get; set; }
    public Color ChartLine { get; set; }
    public Color MenuHover { get; set; }
    public Color MenuActiveText { get; set; }
}

public static class ThemeService
{
    public static string Mode { get; private set; } = "system";
    public static bool IsDark { get; private set; } = false;

    public static event Action? ThemeChanged;

    private static readonly ThemeColors Light = new()
    {
        Bg = Color.FromArgb(245, 246, 247),
        Sidebar = Color.FromArgb(248, 249, 250),
        Card = Color.White,
        Text = Color.FromArgb(34, 34, 34),
        Text2 = Color.FromArgb(102, 102, 102),
        Border = Color.FromArgb(234, 234, 234),
        Accent = Color.FromArgb(45, 140, 240),
        Success = Color.FromArgb(0, 180, 42),
        Warning = Color.FromArgb(255, 125, 0),
        Danger = Color.FromArgb(245, 63, 63),
        ChartLine = Color.FromArgb(45, 140, 240),
        MenuHover = Color.FromArgb(240, 240, 240),
        MenuActiveText = Color.White
    };

    private static readonly ThemeColors Dark = new()
    {
        Bg = Color.FromArgb(18, 18, 18),
        Sidebar = Color.FromArgb(30, 30, 30),
        Card = Color.FromArgb(30, 30, 30),
        Text = Color.FromArgb(224, 224, 224),
        Text2 = Color.FromArgb(170, 170, 170),
        Border = Color.FromArgb(51, 51, 51),
        Accent = Color.FromArgb(70, 166, 255),
        Success = Color.FromArgb(38, 199, 64),
        Warning = Color.FromArgb(255, 149, 0),
        Danger = Color.FromArgb(255, 107, 107),
        ChartLine = Color.FromArgb(70, 166, 255),
        MenuHover = Color.FromArgb(45, 45, 45),
        MenuActiveText = Color.White
    };

    public static ThemeColors Current => IsDark ? Dark : Light;

    public static void SetMode(string mode)
    {
        Mode = mode;
        UpdateIsDark();
        ThemeChanged?.Invoke();
    }

    public static void UpdateIsDark()
    {
        if (Mode == "system")
        {
            IsDark = GetSystemDarkMode();
        }
        else
        {
            IsDark = Mode == "dark";
        }
    }

    public static bool GetSystemDarkMode()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var value = key?.GetValue("AppsUseLightTheme");
            if (value is int i)
                return i == 0;
        }
        catch { }
        return false;
    }

    public static void StartSystemWatcher(System.Windows.Forms.Timer timer)
    {
        bool lastDark = GetSystemDarkMode();
        timer.Interval = 2000;
        timer.Tick += (_, _) =>
        {
            if (Mode != "system") return;
            var current = GetSystemDarkMode();
            if (current != lastDark)
            {
                lastDark = current;
                IsDark = current;
                ThemeChanged?.Invoke();
            }
        };
        timer.Start();
    }
}
