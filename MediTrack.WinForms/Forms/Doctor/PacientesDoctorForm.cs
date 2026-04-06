using MediTrack.WinForms.Config;
using MediTrack.WinForms.Forms.Shared;
using MediTrack.WinForms.Helpers;

namespace MediTrack.WinForms.Forms.Doctor;

public class PacientesDoctorForm : BaseModuleForm
{
    private readonly ApplicationServices _services;
    private readonly AppSession _session;
    private readonly DataGridView _grid = UiFactory.CreateGrid();
    private readonly Label _lblInfo = UiFactory.CreateMutedLabel("Selecciona un paciente para usarlo en medicacion, mediciones, notas e informes.");

    public PacientesDoctorForm(ApplicationServices services, AppSession session) : base("Pacientes asignados")
    {
        _services = services;
        _session = session;

        var page = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
        page.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        page.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var headerCard = AppTheme.CreateMutedPanel();
        headerCard.Padding = new Padding(24);
        var headerLayout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true
        };
        headerLayout.Controls.Add(UiFactory.CreateSectionTitle("Seleccion de contexto clinico"));
        headerLayout.Controls.Add(_lblInfo);
        var btnSelect = UiFactory.CreateButton("Usar paciente seleccionado");
        btnSelect.Click += (_, _) => SelectCurrent();
        headerLayout.Controls.Add(btnSelect);
        headerCard.Controls.Add(headerLayout);

        var card = AppTheme.CreateCardPanel();
        card.Padding = new Padding(20);
        var contentLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
        contentLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        contentLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        contentLayout.Controls.Add(UiFactory.CreateSectionTitle("Listado de pacientes"), 0, 0);
        contentLayout.Controls.Add(_grid, 0, 1);
        card.Controls.Add(contentLayout);

        page.Controls.Add(headerCard, 0, 0);
        page.Controls.Add(card, 0, 1);
        ContentPanel.Controls.Add(page);

        Load += async (_, _) => await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        var items = await _services.PatientService.GetAssignedPatientsSummaryAsync(_session.CurrentUser!.Id);
        _grid.DataSource = items;
        if (_grid.Columns.Contains("PacienteId"))
        {
            _grid.Columns["PacienteId"].Visible = false;
        }
    }

    private void SelectCurrent()
    {
        if (_grid.CurrentRow?.Cells["PacienteId"].Value is Guid patientId)
        {
            _session.SelectedPatientId = patientId;
            MessageBox.Show("Paciente seleccionado para trabajar en el resto de modulos.", "MediTrack");
        }
    }
}
