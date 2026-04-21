using WinBattery.Core;
using WinBattery.Controls;

namespace WinBattery.Pages;

public partial class OverviewPage : UserControl
{
    private Label lblHealthTitle;
    private Label lblHealthValue;
    private TableLayoutPanel grid;
    private Label lblCycleTitle;
    private Label lblCycleValue;
    private Label lblWearTitle;
    private Label lblWearValue;
    private Label lblFullCapTitle;
    private Label lblFullCapValue;
    private Label lblDesignCapTitle;
    private Label lblDesignCapValue;
    private Label lblStatusTitle;
    private Label lblStatusValue;
    private Label lblTipTitle;
    private Label lblTipValue;
    private Label lblChargeTitle;
    private Label lblChargeValue;
    private Label lblTimeTitle;
    private Label lblTimeValue;

    public OverviewPage()
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

        // 电池健康度卡片
        var cardHealth = CreateCard();
        lblHealthTitle = CreateLabelTitle("battery_health");
        lblHealthValue = CreateLabelValue("92%", isSuccess: true);
        cardHealth.Controls.Add(lblHealthTitle);
        cardHealth.Controls.Add(lblHealthValue);
        lblHealthTitle.Location = new Point(16, 12);
        lblHealthValue.Location = new Point(16, 36);

        // 当前电量
        var cardCharge = CreateCard();
        lblChargeTitle = CreateLabelTitle("current_charge");
        lblChargeValue = CreateLabelValue("85%", isAccent: true);
        cardCharge.Controls.Add(lblChargeTitle);
        cardCharge.Controls.Add(lblChargeValue);
        lblChargeTitle.Location = new Point(16, 12);
        lblChargeValue.Location = new Point(16, 36);

        // 剩余时间
        var cardTime = CreateCard();
        lblTimeTitle = CreateLabelTitle("remaining_time");
        lblTimeValue = CreateLabelValue("3h 20m", isAccent: true);
        cardTime.Controls.Add(lblTimeTitle);
        cardTime.Controls.Add(lblTimeValue);
        lblTimeTitle.Location = new Point(16, 12);
        lblTimeValue.Location = new Point(16, 36);

        // 2x2 网格
        grid = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 180,
            ColumnCount = 2,
            RowCount = 2,
            Margin = new Padding(0, 0, 0, 12)
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

        var cardCycle = CreateCard();
        lblCycleTitle = CreateLabelTitle("cycle_count");
        lblCycleValue = CreateLabelValue("36", isAccent: true);
        cardCycle.Controls.Add(lblCycleTitle);
        cardCycle.Controls.Add(lblCycleValue);
        lblCycleTitle.Location = new Point(16, 12);
        lblCycleValue.Location = new Point(16, 36);

        var cardWear = CreateCard();
        lblWearTitle = CreateLabelTitle("battery_wear");
        lblWearValue = CreateLabelValue("8%", isWarning: true);
        cardWear.Controls.Add(lblWearTitle);
        cardWear.Controls.Add(lblWearValue);
        lblWearTitle.Location = new Point(16, 12);
        lblWearValue.Location = new Point(16, 36);

        var cardFullCap = CreateCard();
        lblFullCapTitle = CreateLabelTitle("full_capacity");
        lblFullCapValue = CreateLabelValue("48600 mWh");
        cardFullCap.Controls.Add(lblFullCapTitle);
        cardFullCap.Controls.Add(lblFullCapValue);
        lblFullCapTitle.Location = new Point(16, 12);
        lblFullCapValue.Location = new Point(16, 36);

        var cardDesignCap = CreateCard();
        lblDesignCapTitle = CreateLabelTitle("design_capacity");
        lblDesignCapValue = CreateLabelValue("52800 mWh");
        cardDesignCap.Controls.Add(lblDesignCapTitle);
        cardDesignCap.Controls.Add(lblDesignCapValue);
        lblDesignCapTitle.Location = new Point(16, 12);
        lblDesignCapValue.Location = new Point(16, 36);

        grid.Controls.Add(cardCycle, 0, 0);
        grid.Controls.Add(cardWear, 1, 0);
        grid.Controls.Add(cardFullCap, 0, 1);
        grid.Controls.Add(cardDesignCap, 1, 1);

        // 状态卡片
        var cardStatus = CreateCard();
        cardStatus.Margin = new Padding(0, 0, 0, 12);
        lblStatusTitle = CreateLabelTitle("battery_status");
        lblStatusValue = CreateLabelValue("status_charging");
        cardStatus.Controls.Add(lblStatusTitle);
        cardStatus.Controls.Add(lblStatusValue);
        lblStatusTitle.Location = new Point(16, 12);
        lblStatusValue.Location = new Point(16, 36);
        cardStatus.Height = 70;

        // 建议卡片
        var cardTip = CreateCard();
        lblTipTitle = CreateLabelTitle("health_tip");
        lblTipValue = new Label
        {
            AutoSize = false,
            Width = 500,
            Height = 60,
            Font = new Font("Microsoft YaHei", 10),
            Text = I18nService.T("tip_good"),
            Location = new Point(16, 36)
        };
        cardTip.Controls.Add(lblTipTitle);
        cardTip.Controls.Add(lblTipValue);
        lblTipTitle.Location = new Point(16, 12);
        cardTip.Height = 110;

        // 顶部小网格：电量 + 时间
        var topGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 85,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0, 0, 0, 12)
        };
        topGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        topGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        topGrid.Controls.Add(cardCharge, 0, 0);
        topGrid.Controls.Add(cardTime, 1, 0);

        var panel = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        panel.Controls.Add(cardTip);
        panel.Controls.Add(cardStatus);
        panel.Controls.Add(grid);
        panel.Controls.Add(topGrid);
        panel.Controls.Add(cardHealth);

        // 调整布局顺序（从下到上，因为Dock.Top会堆叠）
        cardHealth.Dock = DockStyle.Top;
        cardHealth.Height = 80;
        topGrid.Dock = DockStyle.Top;
        grid.Dock = DockStyle.Top;
        cardStatus.Dock = DockStyle.Top;
        cardTip.Dock = DockStyle.Top;
        cardTip.Height = 110;

        Controls.Add(panel);
        Height = 600;
        ResumeLayout(false);
    }

    private Panel CreateCard()
    {
        return new CardPanel
        {
            Margin = new Padding(6),
            Padding = new Padding(8)
        };
    }

    private Label CreateLabelTitle(string i18nKey)
    {
        return new Label
        {
            AutoSize = true,
            Font = new Font("Microsoft YaHei", 10),
            Text = I18nService.T(i18nKey)
        };
    }

    private Label CreateLabelValue(string text, bool isSuccess = false, bool isWarning = false, bool isDanger = false, bool isAccent = false)
    {
        var lbl = new Label
        {
            AutoSize = true,
            Font = new Font("Microsoft YaHei", 18, FontStyle.Bold),
            Text = text
        };
        return lbl;
    }

    private void ApplyTheme()
    {
        var c = ThemeService.Current;
        BackColor = c.Bg;
        foreach (Control ctrl in Controls)
        {
            ApplyThemeRecursive(ctrl, c);
        }
    }

    private void ApplyThemeRecursive(Control ctrl, ThemeColors c)
    {
        ctrl.BackColor = c.Card;
        ctrl.ForeColor = c.Text;
        if (ctrl is Label lbl)
        {
            if (lbl.Font.Bold && lbl.Font.Size >= 16)
            {
                // 值标签，根据内容判断颜色
                if (lbl.Text.Contains('%'))
                {
                    var val = lbl.Text.Replace("%", "");
                    if (double.TryParse(val, out var d))
                    {
                        if (d >= 90) lbl.ForeColor = c.Success;
                        else if (d >= 80) lbl.ForeColor = c.Warning;
                        else if (d >= 60) lbl.ForeColor = c.Warning;
                        else lbl.ForeColor = c.Danger;
                    }
                }
                else if (lbl.Name.Contains("Wear") || lbl.Name.Contains("wear"))
                {
                    lbl.ForeColor = c.Warning;
                }
                else if (lbl.Name.Contains("Health") || lbl.Name.Contains("health"))
                {
                    // 后面在刷新数据时设置
                }
                else if (lbl.Name.Contains("Cycle") || lbl.Name.Contains("cycle") || lbl.Name.Contains("Cap") || lbl.Name.Contains("cap"))
                {
                    lbl.ForeColor = c.Accent;
                }
                else if (lbl.Name.Contains("Charge") || lbl.Name.Contains("charge") || lbl.Name.Contains("Time") || lbl.Name.Contains("time"))
                {
                    lbl.ForeColor = c.Accent;
                }
                else
                {
                    lbl.ForeColor = c.Text;
                }
            }
            else
            {
                lbl.ForeColor = c.Text2;
            }
        }
        foreach (Control child in ctrl.Controls)
        {
            ApplyThemeRecursive(child, c);
        }
    }

    public void RefreshData(BatteryInfo info)
    {
        if (info == null) return;
        var c = ThemeService.Current;

        if (info.HealthPercent.HasValue)
        {
            lblHealthValue.Text = $"{info.HealthPercent.Value}%";
            var h = info.HealthPercent.Value;
            lblHealthValue.ForeColor = h >= 90 ? c.Success : h >= 80 ? c.Warning : h >= 60 ? c.Warning : c.Danger;
        }
        else
        {
            lblHealthValue.Text = I18nService.T("unknown");
            lblHealthValue.ForeColor = c.Text2;
        }

        lblCycleValue.Text = info.CycleCount?.ToString() ?? I18nService.T("unknown");
        lblWearValue.Text = info.WearPercent.HasValue ? $"{info.WearPercent.Value}%" : I18nService.T("unknown");
        lblWearValue.ForeColor = c.Warning;
        lblFullCapValue.Text = info.FullChargeCapacity.HasValue ? $"{info.FullChargeCapacity.Value} mWh" : I18nService.T("unknown");
        lblDesignCapValue.Text = info.DesignCapacity.HasValue ? $"{info.DesignCapacity.Value} mWh" : I18nService.T("unknown");

        var statusKey = info.GetStatusText();
        lblStatusValue.Text = I18nService.T($"status_{statusKey.ToLowerInvariant()}") ?? statusKey;

        lblChargeValue.Text = info.EstimatedChargeRemaining.HasValue ? $"{info.EstimatedChargeRemaining.Value}%" : I18nService.T("unknown");

        if (info.EstimatedRunTime.HasValue && info.EstimatedRunTime.Value != 0xFFFFFFFF && info.EstimatedRunTime.Value > 0)
        {
            var mins = info.EstimatedRunTime.Value;
            if (mins > 60)
                lblTimeValue.Text = $"{mins / 60}{I18nService.T("hours")} {mins % 60}{I18nService.T("minutes")}";
            else
                lblTimeValue.Text = $"{mins}{I18nService.T("minutes")}";
        }
        else
        {
            lblTimeValue.Text = I18nService.T("unknown");
        }

        // 健康建议
        if (info.HealthPercent.HasValue)
        {
            var h = info.HealthPercent.Value;
            string tipKey;
            if (h >= 95) tipKey = "tip_excellent";
            else if (h >= 85) tipKey = "tip_good";
            else if (h >= 70) tipKey = "tip_fair";
            else tipKey = "tip_poor";
            lblTipValue.Text = I18nService.T(tipKey);
        }
        else
        {
            lblTipValue.Text = I18nService.T("tip_good");
        }
    }

    private void RefreshTexts()
    {
        lblHealthTitle.Text = I18nService.T("battery_health");
        lblCycleTitle.Text = I18nService.T("cycle_count");
        lblWearTitle.Text = I18nService.T("battery_wear");
        lblFullCapTitle.Text = I18nService.T("full_capacity");
        lblDesignCapTitle.Text = I18nService.T("design_capacity");
        lblStatusTitle.Text = I18nService.T("battery_status");
        lblTipTitle.Text = I18nService.T("health_tip");
        lblChargeTitle.Text = I18nService.T("current_charge");
        lblTimeTitle.Text = I18nService.T("remaining_time");
    }
}
