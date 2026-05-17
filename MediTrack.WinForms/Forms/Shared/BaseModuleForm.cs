using MediTrack.WinForms.Helpers;

namespace MediTrack.WinForms.Forms.Shared;

public class BaseModuleForm : Form
{
    protected readonly TableLayoutPanel RootLayout = new()
    {
        Dock = DockStyle.Fill,
        ColumnCount = 1,
        RowCount = 2,
        Padding = new Padding(24, 20, 24, 24),
        AutoScroll = true,
        BackColor = AppTheme.Background
    };

    protected readonly Label TitleLabel;
    protected readonly Panel ContentPanel = new() { Dock = DockStyle.Fill, BackColor = AppTheme.Background, AutoScroll = true };

    public BaseModuleForm(string title)
    {
        TopLevel = false;
        FormBorderStyle = FormBorderStyle.None;
        Dock = DockStyle.Fill;
        BackColor = AppTheme.Background;
        Text = title;

        RootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        RootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        TitleLabel = UiFactory.CreateTitle(title);
        TitleLabel.Margin = new Padding(0, 0, 0, 18);
        RootLayout.Controls.Add(TitleLabel, 0, 0);
        RootLayout.Controls.Add(ContentPanel, 0, 1);
        Controls.Add(RootLayout);
    }
}
