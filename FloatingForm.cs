using WinBattery.Core;

namespace WinBattery;

public class FloatingForm : Form
{
    private Label lblCharge;
    private Label lblHealth;
    private Label lblStatus;
    private bool isDragging;
    private Point dragStartPoint;
    private System.Windows.Forms.Timer updateTimer;

    public FloatingForm()
    {
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - 160, 80);
        Size = new Size(140, 90);
        ShowInTaskbar = false;
        TopMost = true;
        DoubleBuffered = true;
        Padding = new Padding(8);
        Icon = MainForm.CreateBatteryIcon();

        InitializeComponent();
        ApplyTheme();
        ThemeService.ThemeChanged += ApplyTheme;
        StartUpdateTimer();
    }

    private void InitializeComponent()
    {
        lblCharge = new Label
        {
            Text = "--%",
            Font = new Font("Microsoft YaHei", 16, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(8, 6)
        };

        lblHealth = new Label
        {
            Text = I18nService.T("battery_health") + ": --%",
            Font = new Font("Microsoft YaHei", 9),
            AutoSize = true,
            Location = new Point(8, 36)
        };

        lblStatus = new Label
        {
            Text = "--",
            Font = new Font("Microsoft YaHei", 9),
            AutoSize = true,
            Location = new Point(8, 56)
        };

        Controls.Add(lblCharge);
        Controls.Add(lblHealth);
        Controls.Add(lblStatus);

        // 鼠标拖动
        MouseDown += FloatingForm_MouseDown;
        MouseMove += FloatingForm_MouseMove;
        MouseUp += FloatingForm_MouseUp;
        foreach (Control ctrl in Controls)
        {
            ctrl.MouseDown += FloatingForm_MouseDown;
            ctrl.MouseMove += FloatingForm_MouseMove;
            ctrl.MouseUp += FloatingForm_MouseUp;
        }

        // 右键关闭菜单
        ContextMenuStrip = new ContextMenuStrip();
        var menuShow = new ToolStripMenuItem(I18nService.T("appName"));
        menuShow.Click += (_, _) =>
        {
            if (Program.MainFormInstance != null)
            {
                Program.MainFormInstance.Show();
                Program.MainFormInstance.WindowState = FormWindowState.Normal;
                Program.MainFormInstance.Activate();
            }
        };
        var menuClose = new ToolStripMenuItem("X");
        menuClose.Click += (_, _) => Hide();
        ContextMenuStrip.Items.Add(menuShow);
        ContextMenuStrip.Items.Add(new ToolStripSeparator());
        ContextMenuStrip.Items.Add(menuClose);
    }

    private void FloatingForm_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            isDragging = true;
            dragStartPoint = new Point(e.X, e.Y);
        }
    }

    private void FloatingForm_MouseMove(object? sender, MouseEventArgs e)
    {
        if (isDragging)
        {
            Left = Cursor.Position.X - dragStartPoint.X;
            Top = Cursor.Position.Y - dragStartPoint.Y;
        }
    }

    private void FloatingForm_MouseUp(object? sender, MouseEventArgs e)
    {
        isDragging = false;
    }

    private void StartUpdateTimer()
    {
        updateTimer = new System.Windows.Forms.Timer { Interval = 5000 };
        updateTimer.Tick += (_, _) => RefreshData();
        updateTimer.Start();
        RefreshData();
    }

    public void RefreshData()
    {
        var info = BatteryService.GetBatteryInfo();
        if (info == null) return;

        var charge = info.EstimatedChargeRemaining;
        lblCharge.Text = charge.HasValue ? $"{charge.Value}%" : "--%";
        lblCharge.ForeColor = ThemeService.Current.Accent;

        var health = info.HealthPercent;
        lblHealth.Text = $"{I18nService.T("battery_health")}: {(health.HasValue ? $"{health.Value}%" : "--")}";

        var statusKey = info.GetStatusText().ToLowerInvariant();
        lblStatus.Text = I18nService.T($"status_{statusKey}") ?? info.GetStatusText();
    }

    private void ApplyTheme()
    {
        var c = ThemeService.Current;
        BackColor = c.Card;
        ForeColor = c.Text;
        lblCharge.ForeColor = c.Accent;
        lblHealth.ForeColor = c.Text2;
        lblStatus.ForeColor = c.Text2;

        // 绘制圆角边框
        using var path = GetRoundedRect(new Rectangle(0, 0, Width - 1, Height - 1), 10);
        Region = new Region(path);
    }

    private static System.Drawing.Drawing2D.GraphicsPath GetRoundedRect(Rectangle rect, int radius)
    {
        var path = new System.Drawing.Drawing2D.GraphicsPath();
        int d = radius * 2;
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        updateTimer?.Stop();
        ThemeService.ThemeChanged -= ApplyTheme;
        base.OnFormClosing(e);
    }
}
