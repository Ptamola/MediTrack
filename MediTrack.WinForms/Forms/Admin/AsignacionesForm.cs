using MediTrack.Core.Enums;
using MediTrack.WinForms.Config;
using MediTrack.WinForms.Forms.Shared;
using MediTrack.WinForms.Helpers;

namespace MediTrack.WinForms.Forms.Admin;

public class AsignacionesForm : BaseModuleForm
{
    private readonly ApplicationServices _services;
    private readonly ComboBox _cmbDoctores = UiFactory.CreateComboBox();
    private readonly ComboBox _cmbPacientes = UiFactory.CreateComboBox();
    private readonly DataGridView _grid = UiFactory.CreateGrid();
    private readonly Label _lblHint = UiFactory.CreateParagraphLabel(
        "Relaciona doctores y pacientes para habilitar el seguimiento clínico. Si se asigna un nuevo doctor, el anterior pasa al historial automáticamente.",
        56);

    public AsignacionesForm(ApplicationServices services, AppSession session) : base("Asignación doctor - paciente")
    {
        _services = services;

        var page = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };
        page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46));
        page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 54));

        page.Controls.Add(BuildAssignmentCard(), 0, 0);
        page.Controls.Add(BuildGridCard(), 1, 0);

        ContentPanel.Controls.Add(page);
        Load += async (_, _) => await InitializeAsync();
    }

    private Control BuildAssignmentCard()
    {
        var card = AppTheme.CreateCardPanel();
        card.Padding = new Padding(26, 24, 26, 24);
        card.Margin = new Padding(0, 0, 10, 0);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 7
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        layout.Controls.Add(UiFactory.CreateSectionTitle("Nueva asignación"), 0, 0);
        layout.Controls.Add(_lblHint, 0, 1);
        layout.Controls.Add(BuildFieldPanel("Doctor", _cmbDoctores), 0, 2);
        layout.Controls.Add(BuildFieldPanel("Paciente", _cmbPacientes), 0, 3);

        var info = UiFactory.CreateInfoPanel("La lista de la derecha muestra únicamente las asignaciones activas actuales por paciente.", 82);
        info.Margin = new Padding(0, 8, 0, 0);
        layout.Controls.Add(info, 0, 4);

        var btnAssign = UiFactory.CreateButton("Asignar doctor");
        btnAssign.Width = 200;
        btnAssign.Margin = new Padding(0, 14, 0, 0);
        btnAssign.Click += async (_, _) => await AssignAsync();
        layout.Controls.Add(btnAssign, 0, 5);

        card.Controls.Add(layout);
        return card;
    }

    private Control BuildGridCard()
    {
        var card = AppTheme.CreateCardPanel();
        card.Padding = new Padding(22);
        card.Margin = new Padding(10, 0, 0, 0);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(UiFactory.CreateSectionTitle("Asignaciones activas"), 0, 0);

        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        layout.Controls.Add(_grid, 0, 1);
        card.Controls.Add(layout);
        return card;
    }

    private static Panel BuildFieldPanel(string labelText, Control input)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Top,
            Padding = new Padding(0),
            Margin = new Padding(0, 8, 0, 0),
            Height = 78
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        input.Dock = DockStyle.Top;
        layout.Controls.Add(UiFactory.CreateLabel(labelText), 0, 0);
        layout.Controls.Add(input, 0, 1);
        panel.Controls.Add(layout);
        return panel;
    }

    private async Task InitializeAsync()
    {
        _cmbDoctores.DataSource = await _services.UserService.GetByRoleAsync(UserRole.Doctor);
        _cmbDoctores.DisplayMember = "NombreCompleto";
        _cmbDoctores.ValueMember = "Id";

        _cmbPacientes.DataSource = await _services.UserService.GetByRoleAsync(UserRole.Paciente);
        _cmbPacientes.DisplayMember = "NombreCompleto";
        _cmbPacientes.ValueMember = "Id";

        await ReloadAsync();
    }

    private async Task AssignAsync()
    {
        if (_cmbDoctores.SelectedValue is not Guid doctorId || _cmbPacientes.SelectedValue is not Guid patientId)
        {
            MessageBox.Show("Selecciona un doctor y un paciente válidos.", "MediTrack");
            return;
        }

        var result = await _services.DoctorAssignmentService.AssignAsync(doctorId, patientId);
        MessageBox.Show(result.Message, "MediTrack");
        await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        var users = await _services.UserService.GetAllAsync();
        var assignments = (await _services.DoctorAssignmentService.GetAllAsync())
            .Where(a => a.Activa)
            .GroupBy(a => a.PacienteId)
            .Select(group => group.OrderByDescending(a => a.FechaAsignacion).ThenByDescending(a => a.Id).First())
            .OrderByDescending(a => a.FechaAsignacion)
            .ToList();

        _grid.DataSource = assignments.Select(a => new
        {
            Doctor = users.FirstOrDefault(u => u.Id == a.DoctorId)?.NombreCompleto,
            Paciente = users.FirstOrDefault(u => u.Id == a.PacienteId)?.NombreCompleto,
            Fecha = a.FechaAsignacion.ToString("dd/MM/yyyy")
        }).ToList();

        if (_grid.Columns.Contains("Doctor"))
        {
            _grid.Columns["Doctor"].FillWeight = 36;
            _grid.Columns["Paciente"].FillWeight = 36;
            _grid.Columns["Fecha"].FillWeight = 28;
        }
    }
}
