using MediTrack.Core.Models;
using MediTrack.WinForms.Config;
using MediTrack.WinForms.Forms.Shared;
using MediTrack.WinForms.Helpers;

namespace MediTrack.WinForms.Forms.Patient;

public class EnfermedadesForm : BaseModuleForm
{
    private readonly ApplicationServices _services;
    private readonly AppSession _session;
    private readonly ComboBox _cmbPacientes = UiFactory.CreateComboBox();
    private readonly ComboBox _cmbEnfermedades = UiFactory.CreateComboBox();
    private readonly DateTimePicker _dtpDiagnostico = UiFactory.CreateDatePicker();
    private readonly TextBox _txtObservaciones = UiFactory.CreateTextBox(multiline: true);
    private readonly DataGridView _grid = UiFactory.CreateGrid();
    private readonly Label _lblResumen = UiFactory.CreateParagraphLabel("Consulta tus enfermedades activas y mantén actualizadas las observaciones clínicas.");
    private List<PatientDisease> _items = [];
    private Guid? _selectedRelationId;

    public EnfermedadesForm(ApplicationServices services, AppSession session) : base("Enfermedades crónicas")
    {
        _services = services;
        _session = session;

        var page = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2
        };
        page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
        page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 66));

        var left = BuildFormCard();
        var right = BuildGridCard();

        page.Controls.Add(left, 0, 0);
        page.Controls.Add(right, 1, 0);
        ContentPanel.Controls.Add(page);

        Load += async (_, _) => await InitializeAsync();
    }

    private Control BuildFormCard()
    {
        var left = AppTheme.CreateCardPanel();
        left.Padding = new Padding(24);
        var form = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1
        };
        form.Controls.Add(UiFactory.CreateSectionTitle("Registro de enfermedades"), 0, 0);
        form.Controls.Add(_lblResumen, 0, 1);
        var row = 2;

        if (_session.CurrentUser?.Rol != Core.Enums.UserRole.Paciente)
        {
            form.Controls.Add(UiFactory.CreateLabel("Paciente"), 0, row++);
            form.Controls.Add(_cmbPacientes, 0, row++);
            _cmbPacientes.SelectedIndexChanged += async (_, _) =>
            {
                if (_cmbPacientes.SelectedValue is Guid patientId)
                {
                    _session.SelectedPatientId = patientId;
                }
                await LoadGridAsync();
            };
        }

        form.Controls.Add(UiFactory.CreateLabel("Enfermedad"), 0, row++);
        form.Controls.Add(_cmbEnfermedades, 0, row++);
        form.Controls.Add(UiFactory.CreateLabel("Fecha de diagnóstico"), 0, row++);
        form.Controls.Add(_dtpDiagnostico, 0, row++);
        form.Controls.Add(UiFactory.CreateLabel("Observaciones"), 0, row++);
        form.Controls.Add(_txtObservaciones, 0, row++);

        var note = UiFactory.CreateInfoPanel("Puedes seleccionar una enfermedad del historial para actualizar el diagnóstico u observaciones.");
        form.Controls.Add(note, 0, row++);

        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
        var btnAdd = UiFactory.CreateButton("Añadir enfermedad");
        btnAdd.Click += async (_, _) => await AddAsync();
        var btnUpdate = UiFactory.CreateButton("Guardar observaciones", false);
        btnUpdate.Click += async (_, _) => await UpdateAsync();
        actions.Controls.Add(btnAdd);
        actions.Controls.Add(btnUpdate);
        form.Controls.Add(actions, 0, row++);
        left.Controls.Add(form);
        return left;
    }

    private Control BuildGridCard()
    {
        var right = AppTheme.CreateCardPanel();
        right.Padding = new Padding(20);
        var rightLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
        rightLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        rightLayout.Controls.Add(UiFactory.CreateSectionTitle("Historial de enfermedades"), 0, 0);
        _grid.CellClick += (_, e) => LoadSelected(e.RowIndex);
        rightLayout.Controls.Add(_grid, 0, 1);
        right.Controls.Add(rightLayout);
        return right;
    }

    private async Task InitializeAsync()
    {
        var diseases = await _services.DiseaseService.GetCatalogAsync();
        _cmbEnfermedades.DataSource = diseases;
        _cmbEnfermedades.DisplayMember = "Nombre";
        _cmbEnfermedades.ValueMember = "Id";

        if (_session.CurrentUser?.Rol != Core.Enums.UserRole.Paciente)
        {
            var patients = await PatientAccessHelper.GetAccessiblePatientsAsync(_services, _session);
            _cmbPacientes.DataSource = patients;
            _cmbPacientes.DisplayMember = "NombreCompleto";
            _cmbPacientes.ValueMember = "Id";
            await PatientAccessHelper.EnsureSelectedPatientAsync(_services, _session);
            PatientAccessHelper.ApplySelectedPatient(_cmbPacientes, _session);
            _lblResumen.Text = "Selecciona un paciente y registra o actualiza el seguimiento de sus enfermedades crónicas.";
        }

        await LoadGridAsync();
    }

    private Guid GetPatientId() => _session.CurrentUser?.Rol == Core.Enums.UserRole.Paciente
        ? _session.CurrentUser!.Id
        : _cmbPacientes.SelectedValue is Guid patientId ? patientId : Guid.Empty;

    private async Task AddAsync()
    {
        if (GetPatientId() == Guid.Empty)
        {
            MessageBox.Show("Selecciona un paciente válido.", "MediTrack");
            return;
        }

        if (_cmbEnfermedades.SelectedValue is not Guid diseaseId)
        {
            MessageBox.Show("Selecciona una enfermedad válida.", "MediTrack");
            return;
        }

        var result = await _services.DiseaseService.AssignDiseaseAsync(new PatientDisease
        {
            PacienteId = GetPatientId(),
            EnfermedadId = diseaseId,
            FechaDiagnostico = _dtpDiagnostico.Value.Date,
            Observaciones = _txtObservaciones.Text
        });

        MessageBox.Show(result.Message, "MediTrack");
        if (result.Success)
        {
            ClearSelection();
        }
        await LoadGridAsync();
    }

    private async Task UpdateAsync()
    {
        if (GetPatientId() == Guid.Empty)
        {
            MessageBox.Show("Selecciona un paciente válido.", "MediTrack");
            return;
        }

        if (!_selectedRelationId.HasValue)
        {
            MessageBox.Show("Selecciona una enfermedad del listado para editarla.", "MediTrack");
            return;
        }

        var existing = _items.FirstOrDefault(x => x.Id == _selectedRelationId.Value);
        if (existing == null)
        {
            MessageBox.Show("No se encontró la enfermedad seleccionada.", "MediTrack");
            return;
        }

        existing.FechaDiagnostico = _dtpDiagnostico.Value.Date;
        existing.Observaciones = _txtObservaciones.Text;
        var result = await _services.DiseaseService.UpdateAsync(existing);
        MessageBox.Show(result.Message, "MediTrack");
        if (result.Success)
        {
            ClearSelection();
        }
        await LoadGridAsync();
    }

    private async Task LoadGridAsync()
    {
        var patientId = GetPatientId();
        if (patientId == Guid.Empty)
        {
            _items = [];
            _grid.DataSource = null;
            return;
        }

        _items = await _services.DiseaseService.GetPatientDiseasesAsync(patientId);
        var catalog = await _services.DiseaseService.GetCatalogAsync();
        _grid.DataSource = _items.Join(catalog, pd => pd.EnfermedadId, d => d.Id, (pd, d) => new
        {
            pd.Id,
            Enfermedad = d.Nombre,
            FechaDiagnóstico = pd.FechaDiagnostico.ToString("dd/MM/yyyy"),
            pd.Observaciones
        }).ToList();

        if (_grid.Columns.Contains("Id"))
        {
            _grid.Columns["Id"].Visible = false;
        }
    }

    private void LoadSelected(int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= _items.Count)
        {
            return;
        }

        var selected = _items[rowIndex];
        _selectedRelationId = selected.Id;
        _cmbEnfermedades.SelectedValue = selected.EnfermedadId;
        _dtpDiagnostico.Value = selected.FechaDiagnostico;
        _txtObservaciones.Text = selected.Observaciones;
    }

    private void ClearSelection()
    {
        _selectedRelationId = null;
        _txtObservaciones.Clear();
        _dtpDiagnostico.Value = DateTime.Today;
    }
}
