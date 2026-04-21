using WinBattery.Core;

namespace WinBattery.Pages;

public partial class SettingsPage : UserControl
{
    private Label lblHistoryTitle;
    private Panel historyPanel;
    private Label lblSettingsTitle;
    private Panel settingsPanel;
    private ComboBox cmbLang;
    private ComboBox cmbTheme;
    private CheckBox chkAutoStart;
    private CheckBox chkFloatWindow;
    private NumericUpDown numRefresh;
    private Label lblVersion;
    private Button btnExport;
    private Button btnReport;

    public SettingsPage()
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

        // 历史记录标题
        lblHistoryTitle = new Label
        {
            Text = I18nService.T("history"),
            Font = new Font("Microsoft YaHei", 14, FontStyle.Bold),
            AutoSize = true
        };

        historyPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 140,
            Padding = new Padding(16),
            AutoScroll = true
        };
        // 加载真实历史数据
        LoadHealthHistory();

        var cardHistory = new Panel { Dock = DockStyle.Top, Height = 200, Padding = new Padding(16) };
        var histTitlePanel = new Panel { Dock = DockStyle.Top, Height = 36 };
        histTitlePanel.Controls.Add(lblHistoryTitle);
        lblHistoryTitle.Location = new Point(0, 4);
        cardHistory.Controls.Add(historyPanel);
        cardHistory.Controls.Add(histTitlePanel);

        // 设置标题
        lblSettingsTitle = new Label
        {
            Text = I18nService.T("settings"),
            Font = new Font("Microsoft YaHei", 14, FontStyle.Bold),
            AutoSize = true
        };

        settingsPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16),
            AutoScroll = true
        };

        int y = 0;
        // 语言设置
        var lblLang = CreateLabel("language");
        lblLang.Location = new Point(0, y);
        cmbLang = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 200,
            Location = new Point(200, y)
        };
        cmbLang.Items.AddRange(new object[] { "zh", "en" });
        cmbLang.SelectedItem = I18nService.Lang;
        cmbLang.Format += (_, e) =>
        {
            if (e.ListItem is string s)
                e.Value = s == "zh" ? "简体中文 (Chinese)" : "English";
        };
        cmbLang.SelectedIndexChanged += (_, _) =>
        {
            I18nService.SetLanguage((string)cmbLang.SelectedItem!);
            ConfigService.Config.Language = (string)cmbLang.SelectedItem!;
            ConfigService.Save();
        };
        y += 36;

        // 主题设置
        var lblTheme = CreateLabel("theme");
        lblTheme.Location = new Point(0, y);
        cmbTheme = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 200,
            Location = new Point(200, y)
        };
        cmbTheme.Items.AddRange(new object[] { "system", "light", "dark" });
        cmbTheme.SelectedItem = ThemeService.Mode;
        cmbTheme.Format += (_, e) =>
        {
            if (e.ListItem is string s)
                e.Value = I18nService.T($"theme_{s}");
        };
        cmbTheme.SelectedIndexChanged += (_, _) =>
        {
            ThemeService.SetMode((string)cmbTheme.SelectedItem!);
            ConfigService.Config.ThemeMode = (string)cmbTheme.SelectedItem!;
            ConfigService.Save();
        };
        y += 36;

        // 开机自启
        var lblAutoStart = CreateLabel("auto_start");
        lblAutoStart.Location = new Point(0, y);
        chkAutoStart = new CheckBox
        {
            Location = new Point(200, y),
            Checked = ConfigService.Config.AutoStart
        };
        chkAutoStart.CheckedChanged += (_, _) =>
        {
            ConfigService.Config.AutoStart = chkAutoStart.Checked;
            ConfigService.Save();
            SetAutoStart(chkAutoStart.Checked);
        };
        y += 36;

        // 悬浮窗
        var lblFloat = CreateLabel("float_window");
        lblFloat.Location = new Point(0, y);
        chkFloatWindow = new CheckBox
        {
            Location = new Point(200, y),
            Checked = ConfigService.Config.FloatWindow
        };
        chkFloatWindow.CheckedChanged += (_, _) =>
        {
            ConfigService.Config.FloatWindow = chkFloatWindow.Checked;
            ConfigService.Save();
            ToggleFloatWindow(chkFloatWindow.Checked);
        };
        y += 36;

        // 刷新频率
        var lblRefresh = CreateLabel("refresh_rate");
        lblRefresh.Location = new Point(0, y);
        numRefresh = new NumericUpDown
        {
            Minimum = 1,
            Maximum = 60,
            Value = ConfigService.Config.RefreshInterval,
            Location = new Point(200, y),
            Width = 80
        };
        numRefresh.ValueChanged += (_, _) =>
        {
            ConfigService.Config.RefreshInterval = (int)numRefresh.Value;
            ConfigService.Save();
        };
        y += 36;

        // 版本号
        lblVersion = new Label
        {
            Text = GetRuntimeVersion(),
            Font = new Font("Microsoft YaHei", 10),
            AutoSize = true,
            Location = new Point(0, y)
        };
        y += 40;

        // 按钮
        btnReport = new Button
        {
            Text = I18nService.T("generate_report"),
            Width = 160,
            Height = 32,
            Location = new Point(0, y),
            FlatStyle = FlatStyle.Flat
        };
        btnReport.Click += (_, _) => GenerateBatteryReport();
        y += 44;

        btnExport = new Button
        {
            Text = I18nService.T("data_export"),
            Width = 160,
            Height = 32,
            Location = new Point(0, y),
            FlatStyle = FlatStyle.Flat
        };
        btnExport.Click += (_, _) => ExportData();

        settingsPanel.Controls.Add(lblLang);
        settingsPanel.Controls.Add(cmbLang);
        settingsPanel.Controls.Add(lblTheme);
        settingsPanel.Controls.Add(cmbTheme);
        settingsPanel.Controls.Add(lblAutoStart);
        settingsPanel.Controls.Add(chkAutoStart);
        settingsPanel.Controls.Add(lblFloat);
        settingsPanel.Controls.Add(chkFloatWindow);
        settingsPanel.Controls.Add(lblRefresh);
        settingsPanel.Controls.Add(numRefresh);
        settingsPanel.Controls.Add(lblVersion);
        settingsPanel.Controls.Add(btnReport);
        settingsPanel.Controls.Add(btnExport);

        var cardSettings = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16) };
        var setTitlePanel = new Panel { Dock = DockStyle.Top, Height = 36 };
        setTitlePanel.Controls.Add(lblSettingsTitle);
        lblSettingsTitle.Location = new Point(0, 4);
        cardSettings.Controls.Add(settingsPanel);
        cardSettings.Controls.Add(setTitlePanel);

        var panel = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        panel.Controls.Add(cardSettings);
        panel.Controls.Add(cardHistory);
        Controls.Add(panel);

        ResumeLayout(false);
    }

    private Label CreateLabel(string i18nKey)
    {
        return new Label
        {
            Text = I18nService.T(i18nKey),
            Font = new Font("Microsoft YaHei", 10),
            AutoSize = true
        };
    }

    private void LoadHealthHistory()
    {
        historyPanel.Controls.Clear();
        var items = HistoryService.GetHealthHistory();
        if (items.Count == 0)
        {
            // 还没有历史记录，显示提示
            var lblEmpty = new Label
            {
                Text = I18nService.T("unknown"),
                Font = new Font("Microsoft YaHei", 10),
                AutoSize = true,
                Location = new Point(4, 4)
            };
            historyPanel.Controls.Add(lblEmpty);
            return;
        }
        foreach (var (date, health) in items)
        {
            AddHistoryItem(date, health);
        }
    }

    private void AddHistoryItem(string date, string health)
    {
        var row = new Panel
        {
            Height = 32,
            Dock = DockStyle.Top,
            Padding = new Padding(4)
        };
        var lblDate = new Label
        {
            Text = date,
            Font = new Font("Microsoft YaHei", 10),
            AutoSize = true,
            Location = new Point(4, 4)
        };
        var lblHealth = new Label
        {
            Text = health,
            Font = new Font("Microsoft YaHei", 10, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(300, 4),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        row.Controls.Add(lblHealth);
        row.Controls.Add(lblDate);
        historyPanel.Controls.Add(row);
        // 重新排序让最新的在上面
        row.SendToBack();
    }

    private void GenerateBatteryReport()
    {
        try
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "battery_report.html");
            var psi = new System.Diagnostics.ProcessStartInfo("powercfg", $"/batteryreport /output \"{path}\"")
            {
                CreateNoWindow = true,
                UseShellExecute = false
            };
            using var proc = System.Diagnostics.Process.Start(psi);
            proc?.WaitForExit(10000);
            if (File.Exists(path))
            {
                MessageBox.Show($"{I18nService.T("report_generated")}: {path}", I18nService.T("appName"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, I18nService.T("appName"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ExportData()
    {
        try
        {
            var info = BatteryService.GetBatteryInfo();
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"WinBattery {I18nService.T("data_export")} - {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"{I18nService.T("battery_health")}: {info.HealthPercent?.ToString() ?? I18nService.T("unknown")}");
            sb.AppendLine($"{I18nService.T("battery_wear")}: {info.WearPercent?.ToString() ?? I18nService.T("unknown")}");
            sb.AppendLine($"{I18nService.T("design_capacity")}: {info.DesignCapacity?.ToString() ?? I18nService.T("unknown")} mWh");
            sb.AppendLine($"{I18nService.T("full_capacity")}: {info.FullChargeCapacity?.ToString() ?? I18nService.T("unknown")} mWh");
            sb.AppendLine($"{I18nService.T("cycle_count")}: {info.CycleCount?.ToString() ?? I18nService.T("unknown")}");
            sb.AppendLine($"{I18nService.T("battery_status")}: {I18nService.T($"status_{info.GetStatusText().ToLowerInvariant()}") ?? info.GetStatusText()}");
            sb.AppendLine($"{I18nService.T("manufacturer")}: {info.Manufacturer ?? I18nService.T("unknown")}");
            sb.AppendLine($"{I18nService.T("battery_model")}: {info.Name ?? I18nService.T("unknown")}");
            sb.AppendLine($"{I18nService.T("chemistry")}: {info.GetChemistryText()}");

            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"WinBattery_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
            File.WriteAllText(path, sb.ToString());
            MessageBox.Show($"{I18nService.T("data_export")}: {path}", I18nService.T("appName"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, I18nService.T("appName"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ToggleFloatWindow(bool show)
    {
        if (show)
        {
            if (Program.FloatingFormInstance == null || Program.FloatingFormInstance.IsDisposed)
            {
                Program.FloatingFormInstance = new FloatingForm();
            }
            Program.FloatingFormInstance.Show();
            Program.FloatingFormInstance.RefreshData();
        }
        else
        {
            Program.FloatingFormInstance?.Hide();
        }
    }

    private void SetAutoStart(bool enable)
    {
        try
        {
            var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            if (key == null) return;
            if (enable)
            {
                var exePath = Application.ExecutablePath;
                key.SetValue("WinBattery", $"\"{exePath}\"");
            }
            else
            {
                key.DeleteValue("WinBattery", false);
            }
        }
        catch { }
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
        if (ctrl is Button btn)
        {
            btn.BackColor = c.Accent;
            btn.ForeColor = Color.White;
            btn.FlatAppearance.BorderSize = 0;
        }
        else if (ctrl is ComboBox cmb)
        {
            cmb.BackColor = c.Card;
            cmb.ForeColor = c.Text;
        }
        else if (ctrl is NumericUpDown num)
        {
            num.BackColor = c.Card;
            num.ForeColor = c.Text;
        }
        foreach (Control child in ctrl.Controls)
            ApplyThemeRecursive(child, c);
    }

    private void RefreshTexts()
    {
        lblHistoryTitle.Text = I18nService.T("history");
        lblSettingsTitle.Text = I18nService.T("settings");
        btnReport.Text = I18nService.T("generate_report");
        btnExport.Text = I18nService.T("data_export");
        lblVersion.Text = GetRuntimeVersion();
    }

    private static string GetRuntimeVersion()
    {
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo("git", "rev-list --count HEAD")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = System.Windows.Forms.Application.StartupPath
            };
            using var proc = System.Diagnostics.Process.Start(psi);
            if (proc != null && proc.WaitForExit(3000))
            {
                var count = proc.StandardOutput.ReadToEnd().Trim();
                if (int.TryParse(count, out var c) && c > 0)
                    return $"v1.0.{c}";
            }
        }
        catch { }
        var asmVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        return $"v{asmVersion?.ToString(3) ?? "1.0.0"}";
    }
}
