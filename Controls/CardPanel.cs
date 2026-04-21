using WinBattery.Core;

namespace WinBattery.Controls;

public class CardPanel : Panel
{
    public CardPanel()
    {
        BorderStyle = BorderStyle.None;
        Padding = new Padding(16);
        DoubleBuffered = true;
        Paint += CardPanel_Paint;
    }

    private void CardPanel_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        var c = ThemeService.Current;
        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        using var brush = new SolidBrush(c.Card);
        using var pen = new Pen(c.Border, 1);
        using var path = GetRoundedRect(rect, 10);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.FillPath(brush, path);
        g.DrawPath(pen, path);
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
}
