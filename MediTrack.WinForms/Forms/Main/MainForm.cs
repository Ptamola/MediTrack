using MediTrack.Core.Enums;
using MediTrack.WinForms.Config;
using MediTrack.WinForms.Forms.Admin;
using MediTrack.WinForms.Forms.Doctor;
using MediTrack.WinForms.Forms.Patient;
using MediTrack.WinForms.Forms.Shared;
using MediTrack.WinForms.Helpers;

namespace MediTrack.WinForms.Forms.Main;

/// <summary>
/// Contenedor principal de la aplicacion. Crea la barra lateral y carga formularios segun el rol.
/// </summary>
public class MainForm : Form
{
    private readonly ApplicationServices _services;
    private readonly AppSession _session;
    private readonly Panel _moduleHost = new() { Dock = DockStyle.Fill, BackColor = AppTheme.Background };
    private readonly FlowLayoutPanel _navPanel = new()
    {
        Dock = DockStyle.Fill,
        FlowDirection = FlowDirection.TopDown,
        WrapContents = false,
        AutoScroll = true,
        Padding = new Padding(18),
        BackColor = AppTheme.Sidebar
    };
    private readonly Label _lblPageTitle = new()
    {
        AutoSize = true,
        Font = AppTheme.TitleFont,
        ForeColor = AppTheme.TextPrimary
    };
    private readonly Label _lblPageSubtitle = new()
    {
        AutoSize = false,
        Width = 720,
        Height = 28,
        Font = AppTheme.SmallFont,
        ForeColor = AppTheme.TextSecondary
    };

    private readonly Dictionary<Button, Func<Form>> _navigation = [];
    private Button? _activeButton;

    public MainForm(ApplicationServices services, AppSession session)
    {
        _services = services;
        _session = session;

        AppTheme.StyleForm(this, "MediTrack");
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1366, 820);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 260));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        root.Controls.Add(BuildSidebar(), 0, 0);
        root.Controls.Add(BuildWorkspace(), 1, 0);
        Controls.Add(root);

        BuildNavigation();
    }

    private Control BuildSidebar()
    {
        var sidebar = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = AppTheme.Sidebar
        };

        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 150,
            Padding = new Padding(24, 26, 24, 18),
            BackColor = AppTheme.SidebarAlt
        };

        header.Controls.Add(new Label
        {
            Text = "MediTrack",
            Font = new Font("Segoe UI Semibold", 24, FontStyle.Bold),
            ForeColor = AppTheme.TextOnDark,
            AutoSize = true,
            Dock = DockStyle.Top
        });

        header.Controls.Add(new Label
        {
            Text = "Gestión clínica personal",
            Font = AppTheme.SmallFont,
            ForeColor = Color.FromArgb(191, 219, 254),
            AutoSize = true,
            Dock = DockStyle.Bottom
        });

        var footer = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 110,
            Padding = new Padding(18)
        };

        var btnLogout = UiFactory.CreateButton("Cerrar sesión", false);
        btnLogout.Dock = DockStyle.Bottom;
        btnLogout.Height = 46;
        btnLogout.Click += (_, _) => Close();

        footer.Controls.Add(btnLogout);
        sidebar.Controls.Add(_navPanel);
        sidebar.Controls.Add(footer);
        sidebar.Controls.Add(header);
        return sidebar;
    }

    private Control BuildWorkspace()
    {
        var container = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = AppTheme.Background
        };

        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 110,
            Padding = new Padding(28, 16, 28, 16),
            BackColor = Color.White
        };

        var left = new TableLayoutPanel
        {
            Dock = DockStyle.Left,
            Width = 760,
            ColumnCount = 1,
            RowCount = 2
        };
        left.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        left.Controls.Add(_lblPageTitle, 0, 0);
        left.Controls.Add(_lblPageSubtitle, 0, 1);

        var right = new TableLayoutPanel
        {
            Dock = DockStyle.Right,
            Width = 350,
            ColumnCount = 1,
            RowCount = 2
        };
        right.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        right.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        right.Controls.Add(new Label
        {
            Text = $"{_session.CurrentUser?.NombreCompleto} | {_session.CurrentUser?.Rol}",
            Font = AppTheme.SubtitleFont,
            ForeColor = AppTheme.TextPrimary,
            AutoSize = true,
            TextAlign = ContentAlignment.TopRight
        }, 0, 0);
        right.Controls.Add(new Label
        {
            Text = DateTime.Now.ToString("dddd, dd MMMM yyyy"),
            Font = AppTheme.SmallFont,
            ForeColor = AppTheme.TextSecondary,
            AutoSize = true
        }, 0, 1);

        header.Controls.Add(right);
        header.Controls.Add(left);

        var body = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            BackColor = AppTheme.Background
        };
        body.Controls.Add(_moduleHost);

        container.Controls.Add(body);
        container.Controls.Add(header);
        return container;
    }

    /// <summary>
    /// Registra las opciones de menu permitidas para paciente, doctor o administrador.
    /// </summary>
    private void BuildNavigation()
    {
        var role = _session.CurrentUser?.Rol ?? UserRole.Paciente;
        switch (role)
        {
            case UserRole.Paciente:
                RegisterNav("Resumen", () => new DashboardPacienteForm(_services, _session));
                RegisterNav("Mi perfil", () => new PerfilPacienteForm(_services, _session));
                RegisterNav("Enfermedades", () => new EnfermedadesForm(_services, _session));
                RegisterNav("Medicación", () => new MedicacionForm(_services, _session));
                RegisterNav("Mediciones", () => new MedicionesForm(_services, _session));
                RegisterNav("Notas médicas", () => new NotasMedicasForm(_services, _session));
                RegisterNav("Informes", () => new InformesForm(_services, _session));
                break;
            case UserRole.Doctor:
                RegisterNav("Resumen", () => new DashboardDoctorForm(_services, _session));
                RegisterNav("Pacientes", () => new PacientesDoctorForm(_services, _session));
                RegisterNav("Enfermedades", () => new EnfermedadesForm(_services, _session));
                RegisterNav("Medicación", () => new MedicacionForm(_services, _session));
                RegisterNav("Mediciones", () => new MedicionesForm(_services, _session));
                RegisterNav("Notas médicas", () => new NotasMedicasForm(_services, _session));
                RegisterNav("Informes", () => new InformesForm(_services, _session));
                break;
            case UserRole.Administrador:
                RegisterNav("Resumen", () => new DashboardAdminForm(_services, _session));
                RegisterNav("Usuarios", () => new GestionUsuariosForm(_services, _session));
                RegisterNav("Asignaciones", () => new AsignacionesForm(_services, _session));
                RegisterNav("Informes", () => new InformesForm(_services, _session));
                break;
        }

        if (_navigation.Count > 0)
        {
            var first = _navigation.First();
            ActivateButton(first.Key);
            OpenChild(first.Value());
        }
    }

    private void RegisterNav(string text, Func<Form> formFactory)
    {
        var button = UiFactory.CreateButton(text, false);
        button.Width = 205;
        button.Margin = new Padding(0, 0, 0, 10);
        button.TextAlign = ContentAlignment.MiddleLeft;
        button.BackColor = AppTheme.SidebarAlt;
        button.ForeColor = Color.White;
        button.MouseEnter += (_, _) =>
        {
            if (_activeButton == button)
            {
                return;
            }

            button.BackColor = AppTheme.SecondaryHover;
            button.ForeColor = AppTheme.TextPrimary;
        };
        button.MouseLeave += (_, _) =>
        {
            if (_activeButton == button)
            {
                return;
            }

            button.BackColor = AppTheme.SidebarAlt;
            button.ForeColor = Color.White;
        };
        button.Click += (_, _) =>
        {
            ActivateButton(button);
            OpenChild(formFactory());
        };

        _navigation[button] = formFactory;
        _navPanel.Controls.Add(button);
    }

    private void ActivateButton(Button button)
    {
        if (_activeButton != null)
        {
            _activeButton.BackColor = AppTheme.SidebarAlt;
            _activeButton.ForeColor = Color.White;
        }

        _activeButton = button;
        _activeButton.BackColor = AppTheme.Primary;
        _activeButton.ForeColor = Color.White;
    }

    /// <summary>
    /// Inserta el formulario seleccionado dentro del area central sin abrir ventanas independientes.
    /// </summary>
    private void OpenChild(Form child)
    {
        _moduleHost.Controls.Clear();
        child.TopLevel = false;
        child.Dock = DockStyle.Fill;
        _moduleHost.Controls.Add(child);
        child.Show();

        _lblPageTitle.Text = child.Text;
        _lblPageSubtitle.Text = "Interfaz clínica en tiempo real para una gestión clara, moderna y ordenada.";
    }
}
