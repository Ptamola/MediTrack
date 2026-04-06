using MediTrack.WinForms.Config;
using MediTrack.WinForms.Forms.Main;
using MediTrack.WinForms.Helpers;

namespace MediTrack.WinForms.Forms.Auth;

public class LoginForm : Form
{
    private readonly ApplicationServices _services;
    private readonly AppSession _session;
    private readonly TextBox _txtUsuario = UiFactory.CreateTextBox();
    private readonly TextBox _txtPassword = UiFactory.CreateTextBox(password: true);
    private readonly Label _lblStatus = new()
    {
        AutoSize = false,
        ForeColor = AppTheme.Danger,
        Font = AppTheme.SmallFont,
        Dock = DockStyle.Fill,
        Height = 44,
        TextAlign = ContentAlignment.TopLeft,
        MaximumSize = new Size(420, 0)
    };

    public LoginForm(ApplicationServices services, AppSession session)
    {
        _services = services;
        _session = session;

        AppTheme.StyleForm(this, "MediTrack - Inicio de sesión");
        MinimumSize = new Size(1080, 720);
        Size = new Size(1220, 780);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            BackColor = AppTheme.Background,
            Padding = new Padding(32)
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48));

        root.Controls.Add(BuildBrandingPanel(), 0, 0);
        root.Controls.Add(BuildLoginPanel(), 1, 0);
        Controls.Add(root);
    }

    private Control BuildBrandingPanel()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = AppTheme.Sidebar,
            Padding = new Padding(48)
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            BackColor = AppTheme.Sidebar
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var title = new Label
        {
            Text = "MediTrack",
            AutoSize = true,
            MaximumSize = new Size(560, 0),
            Font = new Font("Segoe UI Semibold", 38, FontStyle.Bold),
            ForeColor = Color.White,
            Margin = new Padding(0, 16, 0, 8)
        };

        var subtitle = new Label
        {
            Text = "Seguimiento clínico moderno para pacientes crónicos",
            AutoSize = true,
            MaximumSize = new Size(540, 0),
            Font = new Font("Segoe UI", 17, FontStyle.Regular),
            ForeColor = Color.FromArgb(191, 219, 254)
        };

        var bullets = new Label
        {
            AutoSize = true,
            MaximumSize = new Size(560, 0),
            Font = new Font("Segoe UI", 13, FontStyle.Regular),
            ForeColor = Color.White,
            Text =
                "- Inicio de sesión por roles" + Environment.NewLine +
                "- Gestión clínica centralizada" + Environment.NewLine +
                "- Informes y PDF integrados" + Environment.NewLine +
                "- Datos locales con arquitectura escalable"
        };

        layout.Controls.Add(title, 0, 0);
        layout.Controls.Add(subtitle, 0, 1);
        layout.Controls.Add(new Panel(), 0, 2);
        layout.Controls.Add(bullets, 0, 3);
        panel.Controls.Add(layout);
        return panel;
    }

    private Control BuildLoginPanel()
    {
        var host = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = AppTheme.Background,
            Padding = new Padding(24)
        };

        var outer = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 3,
            BackColor = AppTheme.Background
        };
        outer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        outer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 480));
        outer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        outer.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        outer.RowStyles.Add(new RowStyle(SizeType.Absolute, 470));
        outer.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

        var card = AppTheme.CreateCardPanel();
        card.Dock = DockStyle.Fill;
        card.Padding = new Padding(30);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 10
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        for (var i = 0; i < 9; i++)
        {
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        }
        layout.RowStyles[8] = new RowStyle(SizeType.Absolute, 52);
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var badge = new Label
        {
            Text = "Acceso seguro",
            AutoSize = true,
            BackColor = Color.FromArgb(219, 234, 254),
            ForeColor = AppTheme.Primary,
            Padding = new Padding(10, 6, 10, 6),
            Margin = new Padding(0, 0, 0, 14)
        };

        var title = UiFactory.CreateTitle("Bienvenido de nuevo");
        var subtitle = UiFactory.CreateMutedLabel("Accede con tu usuario o correo electrónico para continuar.");
        subtitle.AutoSize = true;
        subtitle.MaximumSize = new Size(380, 0);
        subtitle.Margin = new Padding(0, 0, 0, 18);

        _txtUsuario.Dock = DockStyle.Top;
        _txtPassword.Dock = DockStyle.Top;
        _txtUsuario.Margin = new Padding(0, 0, 0, 12);
        _txtPassword.Margin = new Padding(0, 0, 0, 18);

        var btnLogin = UiFactory.CreateButton("Iniciar sesión");
        btnLogin.Dock = DockStyle.Fill;
        btnLogin.Click += async (_, _) => await LoginAsync();

        var btnRegister = UiFactory.CreateButton("Registrarse", false);
        btnRegister.Dock = DockStyle.Fill;
        btnRegister.Click += (_, _) =>
        {
            using var registerForm = new RegisterForm(_services);
            registerForm.ShowDialog(this);
        };

        var lblUsuario = UiFactory.CreateLabel("Usuario o correo electrónico");
        lblUsuario.Margin = new Padding(0, 0, 0, 6);
        var lblPassword = UiFactory.CreateLabel("Contraseña");
        lblPassword.Margin = new Padding(0, 0, 0, 6);

        layout.Controls.Add(badge, 0, 0);
        layout.SetColumnSpan(badge, 2);
        layout.Controls.Add(title, 0, 1);
        layout.SetColumnSpan(title, 2);
        layout.Controls.Add(subtitle, 0, 2);
        layout.SetColumnSpan(subtitle, 2);
        layout.Controls.Add(lblUsuario, 0, 3);
        layout.SetColumnSpan(lblUsuario, 2);
        layout.Controls.Add(_txtUsuario, 0, 4);
        layout.SetColumnSpan(_txtUsuario, 2);
        layout.Controls.Add(lblPassword, 0, 5);
        layout.SetColumnSpan(lblPassword, 2);
        layout.Controls.Add(_txtPassword, 0, 6);
        layout.SetColumnSpan(_txtPassword, 2);
        layout.Controls.Add(btnLogin, 0, 7);
        layout.Controls.Add(btnRegister, 1, 7);
        layout.Controls.Add(_lblStatus, 0, 8);
        layout.SetColumnSpan(_lblStatus, 2);

        card.Controls.Add(layout);
        outer.Controls.Add(card, 1, 1);
        host.Controls.Add(outer);

        host.Resize += (_, _) =>
        {
            outer.ColumnStyles[1].Width = Math.Min(480, Math.Max(360, host.Width - 96));
            outer.RowStyles[1].Height = Math.Min(470, Math.Max(400, host.Height - 80));
        };

        return host;
    }

    private async Task LoginAsync()
    {
        _lblStatus.Text = string.Empty;
        var result = await _services.AuthService.LoginAsync(_txtUsuario.Text.Trim(), _txtPassword.Text);

        if (!result.EsValido || result.Usuario == null)
        {
            _lblStatus.Text = result.Mensaje;
            return;
        }

        _session.SignIn(result.Usuario);
        Hide();

        var main = new MainForm(_services, _session);
        main.FormClosed += (_, _) =>
        {
            _session.SignOut();
            _txtPassword.Clear();
            Show();
        };

        main.Show();
    }
}
