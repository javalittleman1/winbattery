using WinBattery.Core;

namespace WinBattery;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        ConfigService.Load();
        Application.Run(new MainForm());
    }
}
