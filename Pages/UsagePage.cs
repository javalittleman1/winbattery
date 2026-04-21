using WinBattery.Core;

namespace WinBattery.Pages;

public partial class UsagePage : UserControl
{
    private Label lblTitle;
    private Panel listPanel;

    public UsagePage()
    {
        InitializeComponent();
        Dock = DockStyle.Fill;
        I18nService.LanguageChanged += RefreshTexts;
        ThemeService.ThemeChanged += ApplyTheme;
        ApplyTheme();
    }

    private void InitializeComponent()
    {
        SuspendLayout();

        lblTitle = new Label
        {
            Text = I18nService.T("power_usage"),
            Font = new Font("Microsoft YaHei", 14, FontStyle.Bold),
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 12)
        };

        listPanel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(16)
        };

        var card = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16) };
        var titlePanel = new Panel { Dock = DockStyle.Top, Height = 36 };
        titlePanel.Controls.Add(lblTitle);
        lblTitle.Location = new Point(0, 4);
        card.Controls.Add(listPanel);
        card.Controls.Add(titlePanel);

        Controls.Add(card);
        ResumeLayout(false);
    }

    public void RefreshData()
    {
        listPanel.Controls.Clear();
        var data = BatteryService.GetProcessPowerUsage();
        var maxCpu = data.Count > 0 ? data.Max(x => x.CpuPercent) : 1;
        if (maxCpu <= 0) maxCpu = 1;
        int y = 0;
        foreach (var item in data)
        {
            var row = CreateRow(item.ProcessName, $"{item.PowerPercent}%", item.CpuPercent, maxCpu);
            row.Location = new Point(0, y);
            row.Width = listPanel.Width - 32;
            row.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            listPanel.Controls.Add(row);
            y += row.Height;
        }
        ApplyTheme();
    }

    private Panel CreateRow(string name, string percent, int cpuValue, int maxCpu)
    {
        var c = ThemeService.Current;
        var panel = new Panel
        {
            Height = 44,
            Dock = DockStyle.None,
            Margin = new Padding(0, 0, 0, 1)
        };

        var lblName = new Label
        {
            Text = name,
            Font = new Font("Microsoft YaHei", 10),
            AutoSize = true,
            Location = new Point(4, 4)
        };

        var lblPercent = new Label
        {
            Text = percent,
            Font = new Font("Microsoft YaHei", 10, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(300, 4),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };

        // 进度条背景
        var barBg = new Panel
        {
            Height = 4,
            BackColor = Color.FromArgb(60, c.Text2),
            Location = new Point(4, 28),
            Width = 400,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        double ratio = Math.Min(cpuValue / (double)maxCpu, 1.0);
        int fillWidth = Math.Max((int)(barBg.Width * ratio), 2);

        var barFill = new Panel
        {
            Height = 4,
            BackColor = c.Accent,
            Width = fillWidth,
            Location = new Point(0, 0)
        };

        barBg.Controls.Add(barFill);
        panel.Controls.Add(barBg);
        panel.Controls.Add(lblPercent);
        panel.Controls.Add(lblName);

        return panel;
    }

    private void ApplyTheme()
    {
        var c = ThemeService.Current;
        BackColor = c.Bg;
        foreach (Control ctrl in Controls)
            ApplyThemeRecursive(ctrl, c);
    }

    private void ApplyThemeRecursive(Control ctrl, ThemeColors c)
    {
        ctrl.BackColor = c.Card;
        ctrl.ForeColor = c.Text;
        foreach (Control child in ctrl.Controls)
        {
            ApplyThemeRecursive(child, c);
            if (child is Panel p && p.Controls.Count > 0 && p.Controls[0] is Panel bar)
            {
                bar.BackColor = c.Accent;
            }
        }
    }

    private void RefreshTexts()
    {
        lblTitle.Text = I18nService.T("power_usage");
    }
}
