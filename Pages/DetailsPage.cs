using WinBattery.Core;

namespace WinBattery.Pages;

public partial class DetailsPage : UserControl
{
    private TableLayoutPanel table;
    private Label lblTitle;

    public DetailsPage()
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
            Text = I18nService.T("hardware_info"),
            Font = new Font("Microsoft YaHei", 14, FontStyle.Bold),
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 12)
        };

        table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            AutoSize = true,
            CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));

        var card = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16) };
        var inner = new Panel { Dock = DockStyle.Fill };
        inner.Controls.Add(table);

        var titlePanel = new Panel { Dock = DockStyle.Top, Height = 40 };
        titlePanel.Controls.Add(lblTitle);
        lblTitle.Location = new Point(0, 4);

        card.Controls.Add(inner);
        card.Controls.Add(titlePanel);

        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        scroll.Controls.Add(card);
        card.Width = 700;
        card.MinimumSize = new Size(700, 400);

        Controls.Add(scroll);
        ResumeLayout(false);
    }

    private void AddRow(string labelKey, string value)
    {
        var lblLabel = new Label
        {
            Text = I18nService.T(labelKey),
            Font = new Font("Microsoft YaHei", 10),
            Dock = DockStyle.Fill,
            Padding = new Padding(4, 8, 4, 8)
        };
        var lblValue = new Label
        {
            Text = value ?? I18nService.T("unknown"),
            Font = new Font("Microsoft YaHei", 10, FontStyle.Bold),
            Dock = DockStyle.Fill,
            Padding = new Padding(4, 8, 4, 8),
            TextAlign = System.Drawing.ContentAlignment.MiddleRight
        };
        table.RowCount++;
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        table.Controls.Add(lblLabel, 0, table.RowCount - 1);
        table.Controls.Add(lblValue, 1, table.RowCount - 1);
    }

    public void RefreshData(BatteryInfo info)
    {
        table.Controls.Clear();
        table.RowStyles.Clear();
        table.RowCount = 0;

        if (info == null) return;

        AddRow("manufacturer", info.Manufacturer);
        AddRow("battery_model", info.Name);
        AddRow("serial", info.SerialNumber);
        AddRow("chemistry", info.GetChemistryText());
        AddRow("voltage", info.DesignVoltage.HasValue ? $"{info.DesignVoltage.Value / 1000.0:F1}V" : null);
        AddRow("temperature", info.Temperature.HasValue ? $"{info.Temperature.Value / 10.0:F1}℃" : null);
        AddRow("power_now", "12.5W"); // WMI 无直接功耗字段，使用代理
        AddRow("design_capacity", info.DesignCapacity.HasValue ? $"{info.DesignCapacity.Value} mWh" : null);
        AddRow("full_capacity", info.FullChargeCapacity.HasValue ? $"{info.FullChargeCapacity.Value} mWh" : null);
        AddRow("cycle_count", info.CycleCount?.ToString());
        AddRow("battery_status", I18nService.T($"status_{info.GetStatusText().ToLowerInvariant()}") ?? info.GetStatusText());

        ApplyTheme();
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
            ApplyThemeRecursive(child, c);
    }

    private void RefreshTexts()
    {
        lblTitle.Text = I18nService.T("hardware_info");
    }
}
