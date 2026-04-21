using WinBattery.Core;
using WinBattery.Pages;

namespace WinBattery;

public partial class MainForm : Form
{
    private Panel sidebar;
    private Label lblLogo;
    private List<Label> menuItems = new();
    private Panel mainPanel;
    private Panel toolbar;
    private Label lblPageTitle;
    private ComboBox cmbLang;
    private ComboBox cmbTheme;

    private OverviewPage pageOverview;
    private DetailsPage pageDetails;
    private ChartPage pageChart;
    private UsagePage pageUsage;
    private SettingsPage pageSettings;

    private System.Windows.Forms.Timer refreshTimer;
    private System.Windows.Forms.Timer themeWatcherTimer;

    private string currentPage = "overview";

    public MainForm()
    {
        Text = I18nService.T("appName");
        Size = new Size(960, 640);
        MinimumSize = new Size(800, 500);
        StartPosition = FormStartPosition.CenterScreen;
        Icon = SystemIcons.Application;

        InitializeComponent();
        ApplyTheme();
        RefreshTexts();
        StartDataRefresh();
    }

    private void InitializeComponent()
    {
        // 侧边栏
        sidebar = new Panel
        {
            Width = 200,
            Dock = DockStyle.Left,
            Padding = new Padding(0, 16, 0, 16)
        };

        lblLogo = new Label
        {
            Text = I18nService.T("appName"),
            Font = new Font("Microsoft YaHei", 13, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(16, 16),
            Height = 30
        };
        sidebar.Controls.Add(lblLogo);

        string[] pages = { "overview", "details", "chart", "usage", "settings" };
        int y = 60;
        foreach (var p in pages)
        {
            var lbl = new Label
            {
                Text = I18nService.T($"menu_{p}"),
                Tag = p,
                Font = new Font("Microsoft YaHei", 10),
                AutoSize = false,
                Width = 180,
                Height = 40,
                Location = new Point(10, y),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0),
                Cursor = Cursors.Hand
            };
            lbl.Click += MenuItem_Click;
            sidebar.Controls.Add(lbl);
            menuItems.Add(lbl);
            y += 44;
        }
        SetActiveMenu("overview");

        // 顶部工具栏
        toolbar = new Panel
        {
            Height = 56,
            Dock = DockStyle.Top,
            Padding = new Padding(16, 12, 16, 12)
        };

        lblPageTitle = new Label
        {
            Text = I18nService.T("menu_overview"),
            Font = new Font("Microsoft YaHei", 16, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(16, 12)
        };

        cmbLang = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 130,
            Location = new Point(480, 10),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        cmbLang.Items.AddRange(new object[] { "zh", "en" });
        cmbLang.SelectedItem = I18nService.Lang;
        cmbLang.Format += (_, e) =>
        {
            if (e.ListItem is string s)
                e.Value = s == "zh" ? "简体中文" : "English";
        };
        cmbLang.SelectedIndexChanged += CmbLang_SelectedIndexChanged;

        cmbTheme = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 130,
            Location = new Point(620, 10),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        cmbTheme.Items.AddRange(new object[] { "system", "light", "dark" });
        cmbTheme.SelectedItem = ThemeService.Mode;
        cmbTheme.Format += (_, e) =>
        {
            if (e.ListItem is string s)
                e.Value = I18nService.T($"theme_{s}");
        };
        cmbTheme.SelectedIndexChanged += CmbTheme_SelectedIndexChanged;

        toolbar.Controls.Add(lblPageTitle);
        toolbar.Controls.Add(cmbLang);
        toolbar.Controls.Add(cmbTheme);

        // 主内容区
        mainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16)
        };

        pageOverview = new OverviewPage();
        pageDetails = new DetailsPage();
        pageChart = new ChartPage();
        pageUsage = new UsagePage();
        pageSettings = new SettingsPage();

        pageOverview.Visible = true;
        pageDetails.Visible = false;
        pageChart.Visible = false;
        pageUsage.Visible = false;
        pageSettings.Visible = false;

        mainPanel.Controls.Add(pageOverview);
        mainPanel.Controls.Add(pageDetails);
        mainPanel.Controls.Add(pageChart);
        mainPanel.Controls.Add(pageUsage);
        mainPanel.Controls.Add(pageSettings);

        // 布局
        var contentPanel = new Panel { Dock = DockStyle.Fill };
        contentPanel.Controls.Add(mainPanel);
        contentPanel.Controls.Add(toolbar);

        Controls.Add(contentPanel);
        Controls.Add(sidebar);

        // 事件订阅
        I18nService.LanguageChanged += RefreshTexts;
        ThemeService.ThemeChanged += ApplyTheme;
    }

    private void MenuItem_Click(object? sender, EventArgs e)
    {
        if (sender is Label lbl && lbl.Tag is string page)
        {
            SwitchPage(page);
        }
    }

    private void SwitchPage(string page)
    {
        currentPage = page;
        SetActiveMenu(page);
        lblPageTitle.Text = I18nService.T($"menu_{page}");

        pageOverview.Visible = page == "overview";
        pageDetails.Visible = page == "details";
        pageChart.Visible = page == "chart";
        pageUsage.Visible = page == "usage";
        pageSettings.Visible = page == "settings";

        if (page == "usage")
            pageUsage.RefreshData();
    }

    private void SetActiveMenu(string page)
    {
        foreach (var item in menuItems)
        {
            var isActive = item.Tag as string == page;
            item.BackColor = isActive ? ThemeService.Current.Accent : Color.Transparent;
            item.ForeColor = isActive ? ThemeService.Current.MenuActiveText : ThemeService.Current.Text2;
        }
    }

    private void CmbLang_SelectedIndexChanged(object? sender, EventArgs e)
    {
        var lang = (string)cmbLang.SelectedItem!;
        I18nService.SetLanguage(lang);
        ConfigService.Config.Language = lang;
        ConfigService.Save();
    }

    private void CmbTheme_SelectedIndexChanged(object? sender, EventArgs e)
    {
        var mode = (string)cmbTheme.SelectedItem!;
        ThemeService.SetMode(mode);
        ConfigService.Config.ThemeMode = mode;
        ConfigService.Save();
    }

    private void ApplyTheme()
    {
        var c = ThemeService.Current;
        BackColor = c.Bg;
        sidebar.BackColor = c.Sidebar;
        lblLogo.ForeColor = c.Accent;
        toolbar.BackColor = c.Bg;
        lblPageTitle.ForeColor = c.Text;
        mainPanel.BackColor = c.Bg;

        foreach (var item in menuItems)
        {
            var isActive = item.Tag as string == currentPage;
            item.BackColor = isActive ? c.Accent : Color.Transparent;
            item.ForeColor = isActive ? c.MenuActiveText : c.Text2;
        }

        cmbLang.BackColor = c.Card;
        cmbLang.ForeColor = c.Text;
        cmbTheme.BackColor = c.Card;
        cmbTheme.ForeColor = c.Text;

        // 强制所有 CardPanel 重绘以应用新主题边框
        InvalidateCards(this);
    }

    private void InvalidateCards(Control parent)
    {
        foreach (Control ctrl in parent.Controls)
        {
            if (ctrl is Controls.CardPanel cp)
                cp.Invalidate();
            else if (ctrl.Controls.Count > 0)
                InvalidateCards(ctrl);
        }
    }

    private void RefreshTexts()
    {
        Text = I18nService.T("appName");
        lblLogo.Text = I18nService.T("appName");
        lblPageTitle.Text = I18nService.T($"menu_{currentPage}");

        string[] pages = { "overview", "details", "chart", "usage", "settings" };
        for (int i = 0; i < menuItems.Count && i < pages.Length; i++)
        {
            menuItems[i].Text = I18nService.T($"menu_{pages[i]}");
        }
    }

    private void StartDataRefresh()
    {
        refreshTimer = new System.Windows.Forms.Timer();
        refreshTimer.Interval = ConfigService.Config.RefreshInterval * 1000;
        refreshTimer.Tick += (_, _) =>
        {
            var info = BatteryService.GetBatteryInfo();
            pageOverview.RefreshData(info);
            pageDetails.RefreshData(info);
            pageChart.RefreshData(info);
        };
        refreshTimer.Start();

        // 主题监听器
        themeWatcherTimer = new System.Windows.Forms.Timer();
        ThemeService.StartSystemWatcher(themeWatcherTimer);

        // 初始加载
        var initInfo = BatteryService.GetBatteryInfo();
        pageOverview.RefreshData(initInfo);
        pageDetails.RefreshData(initInfo);
        pageChart.RefreshData(initInfo);
        pageUsage.RefreshData();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        refreshTimer?.Stop();
        themeWatcherTimer?.Stop();
        base.OnFormClosing(e);
    }
}
