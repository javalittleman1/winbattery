using WinBattery.Core;

namespace WinBattery;

static class Program
{
    public static MainForm? MainFormInstance { get; set; }
    public static FloatingForm? FloatingFormInstance { get; set; }

    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        ConfigService.Load();
        MainFormInstance = new MainForm();

        if (ConfigService.Config.FloatWindow)
        {
            FloatingFormInstance = new FloatingForm();
            FloatingFormInstance.Show();
        }

        Application.Run(MainFormInstance);
    }
}
