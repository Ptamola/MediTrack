using MediTrack.Core.DTOs;
using MediTrack.WinForms.Config;
using MediTrack.WinForms.Helpers;

namespace MediTrack.WinForms.Forms.Auth;

/// <summary>
/// Formulario publico de registro de pacientes.
/// Solo permite crear cuentas con rol Paciente.
/// </summary>
public class RegisterForm : Form
{
    private readonly ApplicationServices _services;
    private readonly TextBox _txtNombre = UiFactory.CreateTextBox();
    private readonly TextBox _txtApellidos = UiFactory.CreateTextBox();
    private readonly NumericUpDown _numEdad = new()
    {
        Minimum = 1,
        Maximum = 120,
        Value = 30,
        Height = 36,
        Font = new Font("Segoe UI", 10),
        BorderStyle = BorderStyle.FixedSingle,
        Dock = DockStyle.Top
    };
    private readonly TextBox _txtEmail = UiFactory.CreateTextBox();
    private readonly TextBox _txtUsuario = UiFactory.CreateTextBox();
    private readonly TextBox _txtPassword = UiFactory.CreateTextBox(password: true);
    private readonly TextBox _txtConfirmar = UiFactory.CreateTextBox(password: true);
    private readonly Label _lblStatus = new()
    {
        AutoSize = true,
        ForeColor = AppTheme.Danger,
        Font = AppTheme.SmallFont,
        MaximumSize = new Size(720, 0)
    };

    public RegisterForm(ApplicationServices services)
    {
        _services = services;
        AppTheme.StyleForm(this, "Registro de paciente");
        MinimumSize = new Size(980, 720);
        Size = new Size(1060, 780);
        StartPosition = FormStartPosition.CenterParent;

        var root = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(32),
            BackColor = AppTheme.Background
        };

        var card = AppTheme.CreateCardPanel();
        card.Dock = DockStyle.Fill;
        card.Padding = new Padding(32);

        var page = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4
        };
        page.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        page.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        page.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        page.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = new Padding(0, 0, 0, 18)
        };
        header.Controls.Add(UiFactory.CreateTitle("Crear cuenta de paciente"), 0, 0);
        var subtitle = UiFactory.CreateMutedLabel("Completa tus datos para empezar a usar MediTrack.");
        subtitle.Margin = new Padding(0, 0, 0, 0);
        header.Controls.Add(subtitle, 0, 1);

        var fields = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        fields.Controls.Add(BuildColumn(
            ("Nombre", _txtNombre),
            ("Apellidos", _txtApellidos),
            ("Edad", _numEdad),
            ("Correo electrónico", _txtEmail)), 0, 0);

        fields.Controls.Add(BuildColumn(
            ("Nombre de usuario", _txtUsuario),
            ("Contraseña", _txtPassword),
            ("Confirmar contraseña", _txtConfirmar)), 1, 0);

        var footer = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            Margin = new Padding(0, 16, 0, 0)
        };
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var btnRegister = UiFactory.CreateButton("Crear cuenta");
        btnRegister.Dock = DockStyle.Fill;
        btnRegister.Click += async (_, _) => await RegisterAsync();
        var btnCancel = UiFactory.CreateButton("Cancelar", false);
        btnCancel.Dock = DockStyle.Fill;
        btnCancel.Click += (_, _) => Close();

        footer.Controls.Add(btnRegister, 0, 0);
        footer.Controls.Add(btnCancel, 1, 0);

        page.Controls.Add(header, 0, 0);
        page.Controls.Add(fields, 0, 1);
        page.Controls.Add(_lblStatus, 0, 2);
        page.Controls.Add(footer, 0, 3);

        card.Controls.Add(page);
        root.Controls.Add(card);
        Controls.Add(root);
    }

    private static Control BuildColumn(params (string label, Control control)[] fields)
    {
        var panel = AppTheme.CreateMutedPanel();
        panel.Padding = new Padding(20);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = fields.Length * 2
        };

        var row = 0;
        foreach (var field in fields)
        {
            var label = UiFactory.CreateLabel(field.label);
            label.Margin = new Padding(0, row == 0 ? 0 : 8, 0, 6);
            field.control.Dock = DockStyle.Top;
            field.control.Margin = new Padding(0, 0, 0, 10);
            layout.Controls.Add(label, 0, row++);
            layout.Controls.Add(field.control, 0, row++);
        }

        panel.Controls.Add(layout);
        return panel;
    }

    /// <summary>
    /// Envia los datos al servicio de autenticacion para crear usuario y perfil de paciente.
    /// </summary>
    private async Task RegisterAsync()
    {
        var request = new RegisterRequest
        {
            Nombre = _txtNombre.Text,
            Apellidos = _txtApellidos.Text,
            Edad = (int)_numEdad.Value,
            Email = _txtEmail.Text,
            NombreUsuario = _txtUsuario.Text,
            Password = _txtPassword.Text,
            ConfirmarPassword = _txtConfirmar.Text
        };

        var result = await _services.AuthService.RegisterPatientAsync(request);
        _lblStatus.ForeColor = result.Success ? AppTheme.Success : AppTheme.Danger;
        _lblStatus.Text = result.Message;

        if (result.Success)
        {
            MessageBox.Show("Registro completado. Ya puedes iniciar sesión.", "MediTrack", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        }
    }
}
