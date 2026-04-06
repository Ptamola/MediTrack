namespace MediTrack.WinForms.Helpers;

public static class UiFactory
{
    public static Label CreateTitle(string text) => new()
    {
        Text = text,
        AutoSize = true,
        Font = AppTheme.TitleFont,
        ForeColor = AppTheme.TextPrimary,
        Margin = new Padding(0, 0, 0, 8)
    };

    public static Label CreateSectionTitle(string text) => new()
    {
        Text = text,
        AutoSize = true,
        Font = AppTheme.SubtitleFont,
        ForeColor = AppTheme.TextPrimary,
        Margin = new Padding(0, 0, 0, 10)
    };

    public static Label CreateLabel(string text) => new()
    {
        Text = text,
        AutoSize = true,
        Font = AppTheme.SmallFont,
        ForeColor = AppTheme.TextSecondary,
        Margin = new Padding(0, 8, 0, 6)
    };

    public static Label CreateMutedLabel(string text) => new()
    {
        Text = text,
        AutoSize = true,
        Font = AppTheme.BodyFont,
        ForeColor = AppTheme.TextSecondary,
        Margin = new Padding(0)
    };

    public static Label CreateParagraphLabel(string text, int height = 52) => new()
    {
        Text = text,
        Dock = DockStyle.Top,
        AutoSize = false,
        Height = height,
        Font = AppTheme.BodyFont,
        ForeColor = AppTheme.TextSecondary,
        Margin = new Padding(0)
    };

    public static Panel CreateInfoPanel(string text, int height = 88)
    {
        var panel = AppTheme.CreateMutedPanel();
        panel.Height = height;
        panel.Dock = DockStyle.Top;
        panel.Padding = new Padding(16);
        panel.Controls.Add(CreateParagraphLabel(text, height - 28));
        return panel;
    }

    public static TextBox CreateTextBox(bool multiline = false, bool password = false)
    {
        var box = new TextBox
        {
            Multiline = multiline,
            PasswordChar = password ? '*' : '\0',
            Width = 0,
            Height = multiline ? 96 : 36,
            BorderStyle = BorderStyle.FixedSingle,
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
        };

        AppTheme.ApplyTextInputStyle(box);
        return box;
    }

    public static ComboBox CreateComboBox()
    {
        var combo = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 0,
            Height = 36,
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
        };

        AppTheme.ApplyTextInputStyle(combo);
        return combo;
    }

    public static DateTimePicker CreateDatePicker()
    {
        var picker = new DateTimePicker
        {
            Format = DateTimePickerFormat.Short,
            Width = 0,
            Height = 36,
            Font = AppTheme.BodyFont,
            CalendarForeColor = AppTheme.TextPrimary,
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
        };

        return picker;
    }

    public static Button CreateButton(string text, bool primary = true)
    {
        var button = new Button
        {
            Text = text,
            Width = 180
        };

        AppTheme.ApplyButtonStyle(button, primary);
        return button;
    }

    public static Button CreateDangerButton(string text)
    {
        var button = new Button
        {
            Text = text,
            Width = 180
        };

        AppTheme.ApplyButtonStyle(button, primary: false, danger: true);
        return button;
    }

    public static DataGridView CreateGrid()
    {
        var grid = new DataGridView();
        AppTheme.ApplyGridStyle(grid);
        return grid;
    }

    public static ListBox CreateListBox()
    {
        var list = new ListBox();
        AppTheme.ApplyListStyle(list);
        return list;
    }

    public static FlowLayoutPanel CreateVerticalFormPanel() => new()
    {
        Dock = DockStyle.Fill,
        FlowDirection = FlowDirection.TopDown,
        WrapContents = false,
        AutoScroll = true,
        Padding = new Padding(0),
        Margin = new Padding(0)
    };

    public static Panel CreateMetricCard(string title, string value, string subtitle, Color accent)
    {
        var card = AppTheme.CreateMutedPanel();
        card.Padding = new Padding(18, 16, 18, 16);
        card.MinimumSize = new Size(220, 120);

        var marker = new Panel
        {
            Width = 6,
            Dock = DockStyle.Left,
            BackColor = accent
        };

        var titleLabel = new Label
        {
            Text = title,
            AutoSize = true,
            MaximumSize = new Size(260, 0),
            Font = AppTheme.SmallFont,
            ForeColor = AppTheme.TextSecondary,
            Margin = new Padding(0, 0, 0, 6)
        };

        var valueLabel = new Label
        {
            Text = value,
            AutoSize = true,
            MaximumSize = new Size(260, 0),
            Font = new Font("Segoe UI Semibold", 18, FontStyle.Bold),
            ForeColor = AppTheme.TextPrimary,
            Margin = new Padding(0, 0, 0, 4)
        };

        var subtitleLabel = new Label
        {
            Text = subtitle,
            AutoSize = true,
            MaximumSize = new Size(260, 0),
            Font = AppTheme.SmallFont,
            ForeColor = AppTheme.TextSecondary
        };

        var content = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(14, 0, 0, 0),
            BackColor = AppTheme.SurfaceMuted
        };
        content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        content.Controls.Add(titleLabel, 0, 0);
        content.Controls.Add(valueLabel, 0, 1);
        content.Controls.Add(subtitleLabel, 0, 2);
        card.Controls.Add(content);
        card.Controls.Add(marker);
        return card;
    }
}
