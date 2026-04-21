using WinBattery.Core;

namespace WinBattery;

static class Program
{
    public static MainForm? MainFormInstance { get; set; }
    public static FloatingForm? FloatingFormInstance { get; set; }

    [STAThread]
    static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        ConfigService.Load();
        MainFormInstance = new MainForm();

        var pageArg = args.FirstOrDefault(a => a.StartsWith("--page="));
        if (pageArg != null)
        {
            var page = pageArg[("--page=".Length)..];
            MainFormInstance.Shown += (_, _) => MainFormInstance.SwitchToPage(page);
        }

        if (ConfigService.Config.FloatWindow)
        {
            FloatingFormInstance = new FloatingForm();
            FloatingFormInstance.Show();
        }

        Application.Run(MainFormInstance);
    }
}
