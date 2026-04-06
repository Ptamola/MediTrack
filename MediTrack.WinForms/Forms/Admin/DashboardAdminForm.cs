using MediTrack.Core.Enums;
using MediTrack.WinForms.Config;
using MediTrack.WinForms.Forms.Shared;
using MediTrack.WinForms.Helpers;

namespace MediTrack.WinForms.Forms.Admin;

public class DashboardAdminForm : BaseModuleForm
{
    private readonly ApplicationServices _services;
    private readonly DataGridView _grid = UiFactory.CreateGrid();
    private readonly Label _lblUsers = new();
    private readonly Label _lblPatients = new();
    private readonly Label _lblDoctors = new();
    private readonly Label _lblAssignments = new();

    public DashboardAdminForm(ApplicationServices services, AppSession session) : base("Dashboard de administración")
    {
        _services = services;

        var page = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
        page.RowStyles.Add(new RowStyle(SizeType.Absolute, 140));
        page.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var metrics = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4 };
        for (var i = 0; i < 4; i++)
        {
            metrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        }

        metrics.Controls.Add(UiFactory.CreateMetricCard("Usuarios", "0", "Total registrados", AppTheme.Primary), 0, 0);
        metrics.Controls.Add(UiFactory.CreateMetricCard("Pacientes", "0", "Cuentas activas e inactivas", AppTheme.Accent), 1, 0);
        metrics.Controls.Add(UiFactory.CreateMetricCard("Doctores", "0", "Profesionales cargados", AppTheme.Success), 2, 0);
        metrics.Controls.Add(UiFactory.CreateMetricCard("Asignaciones", "0", "Pacientes con doctor activo", AppTheme.Warning), 3, 0);
        Link(metrics.GetControlFromPosition(0, 0), _lblUsers);
        Link(metrics.GetControlFromPosition(1, 0), _lblPatients);
        Link(metrics.GetControlFromPosition(2, 0), _lblDoctors);
        Link(metrics.GetControlFromPosition(3, 0), _lblAssignments);

        var card = AppTheme.CreateCardPanel();
        card.Padding = new Padding(18);
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(UiFactory.CreateSectionTitle("Catálogo clínico de enfermedades crónicas"), 0, 0);
        layout.Controls.Add(_grid, 0, 1);
        card.Controls.Add(layout);

        page.Controls.Add(metrics, 0, 0);
        page.Controls.Add(card, 0, 1);
        ContentPanel.Controls.Add(page);

        Load += async (_, _) => await LoadDataAsync();
    }

    private static void Link(Control? card, Label source)
    {
        if (card == null)
        {
            return;
        }

        foreach (Control control in card.Controls)
        {
            if (control is not TableLayoutPanel panel)
            {
                continue;
            }

            foreach (Control child in panel.Controls)
            {
                if (child is Label label && label.Font.Size >= 18)
                {
                    source.TextChanged += (_, _) => label.Text = source.Text;
                    label.Text = source.Text;
                    return;
                }
            }
        }
    }

    private async Task LoadDataAsync()
    {
        var users = await _services.UserService.GetAllAsync();
        var assignments = await _services.DoctorAssignmentService.GetAllAsync();
        var currentAssignments = assignments
            .Where(a => a.Activa)
            .GroupBy(a => a.PacienteId)
            .Select(group => group.OrderByDescending(a => a.FechaAsignacion).ThenByDescending(a => a.Id).First())
            .ToList();

        _lblUsers.Text = users.Count.ToString();
        _lblPatients.Text = users.Count(u => u.Rol == UserRole.Paciente).ToString();
        _lblDoctors.Text = users.Count(u => u.Rol == UserRole.Doctor).ToString();
        _lblAssignments.Text = currentAssignments.Count.ToString();

        _grid.DataSource = (await _services.DiseaseService.GetCatalogAsync())
            .Select(d => new { d.Nombre, d.Descripcion })
            .ToList();
    }
}
