using WinBattery.Core;

namespace WinBattery.Pages;

public partial class ChartPage : UserControl
{
    private Label lblTitle;
    private Panel chartPanel;
    private Label lblLastChargeTitle;
    private Label lblLastChargeValue;

    // 模拟历史数据
    private readonly List<int> chargeHistory = new();

    public ChartPage()
    {
        InitializeComponent();
        Dock = DockStyle.Fill;
        I18nService.LanguageChanged += RefreshTexts;
        ThemeService.ThemeChanged += ApplyTheme;

        // 生成模拟数据
        var rand = new Random(42);
        for (int i = 0; i < 36; i++)
            chargeHistory.Add(20 + (int)(i * 2.2) + rand.Next(-3, 4));
        ApplyTheme();
    }

    private void InitializeComponent()
    {
        SuspendLayout();

        lblTitle = new Label
        {
            Text = I18nService.T("charge_curve"),
            Font = new Font("Microsoft YaHei", 14, FontStyle.Bold),
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 12)
        };

        chartPanel = new Panel
        {
            Height = 220,
            Dock = DockStyle.Top,
            Margin = new Padding(0, 0, 0, 12)
        };
        chartPanel.Paint += ChartPanel_Paint;

        var cardChart = new Panel { Dock = DockStyle.Top, Height = 280, Padding = new Padding(16) };
        var titlePanel = new Panel { Dock = DockStyle.Top, Height = 36 };
        titlePanel.Controls.Add(lblTitle);
        lblTitle.Location = new Point(0, 4);
        cardChart.Controls.Add(chartPanel);
        cardChart.Controls.Add(titlePanel);

        lblLastChargeTitle = CreateLabelTitle("last_charge");
        lblLastChargeValue = new Label
        {
            AutoSize = false,
            Width = 600,
            Height = 30,
            Font = new Font("Microsoft YaHei", 10),
            Text = I18nService.T("charge_log")
        };
        var cardLog = new Panel { Dock = DockStyle.Top, Height = 100, Padding = new Padding(16) };
        cardLog.Controls.Add(lblLastChargeValue);
        cardLog.Controls.Add(lblLastChargeTitle);
        lblLastChargeTitle.Location = new Point(16, 12);
        lblLastChargeValue.Location = new Point(16, 40);

        var panel = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        panel.Controls.Add(cardLog);
        panel.Controls.Add(cardChart);
        Controls.Add(panel);

        ResumeLayout(false);
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

    private void ChartPanel_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        var c = ThemeService.Current;
        var rect = chartPanel.ClientRectangle;
        rect.Inflate(-20, -20);

        // 背景
        using var bgBrush = new SolidBrush(c.Card);
        g.FillRectangle(bgBrush, rect);

        // 网格线
        using var gridPen = new Pen(Color.FromArgb(40, c.Text2), 1);
        for (int i = 0; i <= 5; i++)
        {
            int y = rect.Top + rect.Height * i / 5;
            g.DrawLine(gridPen, rect.Left, y, rect.Right, y);
        }
        for (int i = 0; i <= 6; i++)
        {
            int x = rect.Left + rect.Width * i / 6;
            g.DrawLine(gridPen, x, rect.Top, x, rect.Bottom);
        }

        if (chargeHistory.Count < 2) return;

        // 绘制曲线
        using var linePen = new Pen(c.ChartLine, 2);
        using var fillBrush = new SolidBrush(Color.FromArgb(40, c.ChartLine));

        var points = new List<PointF>();
        float minVal = chargeHistory.Min();
        float maxVal = chargeHistory.Max();
        float range = Math.Max(maxVal - minVal, 1);

        for (int i = 0; i < chargeHistory.Count; i++)
        {
            float x = rect.Left + (float)i / (chargeHistory.Count - 1) * rect.Width;
            float y = rect.Bottom - (chargeHistory[i] - minVal) / range * rect.Height;
            points.Add(new PointF(x, y));
        }

        if (points.Count > 1)
        {
            // 填充区域
            var fillPoints = new List<PointF>(points);
            fillPoints.Add(new PointF(rect.Right, rect.Bottom));
            fillPoints.Add(new PointF(rect.Left, rect.Bottom));
            g.FillPolygon(fillBrush, fillPoints.ToArray());

            // 线条
            g.DrawCurve(linePen, points.ToArray(), 0.3f);
        }

        // 坐标轴标签
        using var font = new Font("Microsoft YaHei", 8);
        using var textBrush = new SolidBrush(c.Text2);
        g.DrawString("0h", font, textBrush, rect.Left, rect.Bottom + 2);
        g.DrawString("3h", font, textBrush, rect.Right - 16, rect.Bottom + 2);
        g.DrawString("100%", font, textBrush, rect.Left - 32, rect.Top);
        g.DrawString("0%", font, textBrush, rect.Left - 20, rect.Bottom - 12);
    }

    public void RefreshData(BatteryInfo info)
    {
        // 可以在这里添加实时数据点
        if (info?.EstimatedChargeRemaining.HasValue == true)
        {
            chargeHistory.Add((int)info.EstimatedChargeRemaining.Value);
            if (chargeHistory.Count > 72) chargeHistory.RemoveAt(0);
        }
        chartPanel.Invalidate();
    }

    private void ApplyTheme()
    {
        var c = ThemeService.Current;
        BackColor = c.Bg;
        foreach (Control ctrl in Controls)
            ApplyThemeRecursive(ctrl, c);
        chartPanel.Invalidate();
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
        lblTitle.Text = I18nService.T("charge_curve");
        lblLastChargeTitle.Text = I18nService.T("last_charge");
        lblLastChargeValue.Text = I18nService.T("charge_log");
    }
}
