using System.Drawing.Drawing2D;

namespace MediTrack.WinForms.Helpers;

public static class AppTheme
{
    public static Color Sidebar = Color.FromArgb(15, 23, 42);
    public static Color SidebarAlt = Color.FromArgb(30, 41, 59);
    public static Color Primary = Color.FromArgb(37, 99, 235);
    public static Color PrimaryHover = Color.FromArgb(59, 130, 246);
    public static Color SecondaryHover = Color.FromArgb(167, 243, 208);
    public static Color SecondaryPressed = Color.FromArgb(134, 239, 172);
    public static Color Accent = Color.FromArgb(14, 116, 144);
    public static Color Success = Color.FromArgb(22, 163, 74);
    public static Color Danger = Color.FromArgb(220, 38, 38);
    public static Color Warning = Color.FromArgb(245, 158, 11);
    public static Color Background = Color.FromArgb(241, 245, 249);
    public static Color Surface = Color.White;
    public static Color SurfaceMuted = Color.FromArgb(248, 250, 252);
    public static Color Border = Color.FromArgb(226, 232, 240);
    public static Color TextPrimary = Color.FromArgb(15, 23, 42);
    public static Color TextSecondary = Color.FromArgb(100, 116, 139);
    public static Color TextOnDark = Color.White;

    public static Font DisplayFont = new("Segoe UI Semibold", 24, FontStyle.Bold);
    public static Font TitleFont = new("Segoe UI Semibold", 20, FontStyle.Bold);
    public static Font SubtitleFont = new("Segoe UI Semibold", 11, FontStyle.Bold);
    public static Font BodyFont = new("Segoe UI", 10, FontStyle.Regular);
    public static Font SmallFont = new("Segoe UI", 9, FontStyle.Regular);

    public static void StyleForm(Form form, string title)
    {
        form.Text = title;
        form.BackColor = Background;
        form.Font = BodyFont;
        form.StartPosition = FormStartPosition.CenterScreen;
    }

    public static Panel CreateCardPanel()
    {
        var panel = new Panel
        {
            BackColor = Surface,
            Padding = new Padding(18),
            Margin = new Padding(0),
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.FixedSingle
        };

        panel.Resize += (_, _) => SetRoundedRegion(panel, 18);
        return panel;
    }

    public static Panel CreateMutedPanel()
    {
        var panel = new Panel
        {
            BackColor = SurfaceMuted,
            Padding = new Padding(14),
            Margin = new Padding(0),
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.FixedSingle
        };

        panel.Resize += (_, _) => SetRoundedRegion(panel, 16);
        return panel;
    }

    public static void ApplyButtonStyle(Button button, bool primary = true, bool danger = false)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseDownBackColor = danger ? Danger : primary ? Primary : SecondaryPressed;
        button.FlatAppearance.MouseOverBackColor = danger ? Danger : primary ? PrimaryHover : SecondaryHover;
        button.Height = 44;
        button.Cursor = Cursors.Hand;
        button.BackColor = danger ? Danger : primary ? Primary : SurfaceMuted;
        button.ForeColor = primary || danger ? Color.White : TextPrimary;
        button.Font = SubtitleFont;
        button.Padding = new Padding(12, 0, 12, 0);
        button.Resize += (_, _) => SetRoundedRegion(button, 12);
    }

    public static void ApplyTextInputStyle(Control control)
    {
        control.BackColor = Color.White;
        control.ForeColor = TextPrimary;
        control.Font = BodyFont;
        control.Margin = new Padding(0, 0, 0, 14);
        control.MinimumSize = control is TextBoxBase textBox && textBox.Multiline
            ? new Size(180, 96)
            : new Size(180, 36);
    }

    public static void ApplyGridStyle(DataGridView grid)
    {
        grid.BackgroundColor = Surface;
        grid.BorderStyle = BorderStyle.None;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
        grid.ReadOnly = true;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.RowHeadersVisible = false;
        grid.EnableHeadersVisualStyles = false;
        grid.Dock = DockStyle.Fill;
        grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.GridColor = Border;
        grid.ColumnHeadersDefaultCellStyle.BackColor = Primary;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        grid.ColumnHeadersDefaultCellStyle.Font = SubtitleFont;
        grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 10, 8, 10);
        grid.ColumnHeadersHeight = 48;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        grid.DefaultCellStyle.BackColor = Surface;
        grid.DefaultCellStyle.ForeColor = TextPrimary;
        grid.DefaultCellStyle.Padding = new Padding(8, 8, 8, 8);
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
        grid.DefaultCellStyle.SelectionForeColor = TextPrimary;
        grid.AlternatingRowsDefaultCellStyle.BackColor = SurfaceMuted;
    }

    public static void ApplyListStyle(ListBox listBox)
    {
        listBox.BorderStyle = BorderStyle.None;
        listBox.BackColor = SurfaceMuted;
        listBox.ForeColor = TextPrimary;
        listBox.Font = BodyFont;
        listBox.IntegralHeight = false;
        listBox.ItemHeight = 22;
    }

    public static void SetRoundedRegion(Control control, int radius = 14)
    {
        if (control.Width <= 0 || control.Height <= 0)
        {
            return;
        }

        using var path = new GraphicsPath();
        var rect = new Rectangle(0, 0, control.Width, control.Height);
        path.AddArc(rect.Left, rect.Top, radius, radius, 180, 90);
        path.AddArc(rect.Right - radius, rect.Top, radius, radius, 270, 90);
        path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
        path.AddArc(rect.Left, rect.Bottom - radius, radius, radius, 90, 90);
        path.CloseFigure();
        control.Region = new Region(path);
    }
}
