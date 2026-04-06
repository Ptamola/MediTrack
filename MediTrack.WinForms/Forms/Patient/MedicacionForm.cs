using MediTrack.Core.Models;
using MediTrack.WinForms.Config;
using MediTrack.WinForms.Forms.Shared;
using MediTrack.WinForms.Helpers;

namespace MediTrack.WinForms.Forms.Patient;

public class MedicacionForm : BaseModuleForm
{
    private readonly ApplicationServices _services;
    private readonly AppSession _session;
    private readonly ComboBox _cmbPacientes = UiFactory.CreateComboBox();
    private readonly TextBox _txtNombre = UiFactory.CreateTextBox();
    private readonly TextBox _txtDosis = UiFactory.CreateTextBox();
    private readonly TextBox _txtFrecuencia = UiFactory.CreateTextBox();
    private readonly TextBox _txtHorario = UiFactory.CreateTextBox();
    private readonly TextBox _txtObservaciones = UiFactory.CreateTextBox(multiline: true);
    private readonly DateTimePicker _dtpInicio = UiFactory.CreateDatePicker();
    private readonly DateTimePicker _dtpFin = UiFactory.CreateDatePicker();
    private readonly CheckBox _chkActivo = new() { Text = "Activo", Checked = true, AutoSize = true };
    private readonly DataGridView _grid = UiFactory.CreateGrid();
    private readonly Label _lblHint = UiFactory.CreateParagraphLabel("Gestiona tu medicación con una vista clara, ordenada y fácil de revisar.");
    private List<Medication> _items = [];
    private Guid? _selectedMedicationId;

    public MedicacionForm(ApplicationServices services, AppSession session) : base("Gestión de medicación")
    {
        _services = services;
        _session = session;

        var page = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2
        };
        page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36));
        page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64));

        var formCard = BuildFormCard();
        var gridCard = BuildGridCard();

        page.Controls.Add(formCard, 0, 0);
        page.Controls.Add(gridCard, 1, 0);
        ContentPanel.Controls.Add(page);

        Load += async (_, _) => await InitializeAsync();
    }

    private Control BuildFormCard()
    {
        var formCard = AppTheme.CreateCardPanel();
        formCard.Padding = new Padding(24);
        var form = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2
        };
        form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        form.Controls.Add(UiFactory.CreateSectionTitle("Ficha de medicamento"), 0, 0);
        form.SetColumnSpan(form.GetControlFromPosition(0, 0)!, 2);
        form.Controls.Add(_lblHint, 0, 1);
        form.SetColumnSpan(_lblHint, 2);
        var row = 2;

        if (_session.CurrentUser?.Rol != Core.Enums.UserRole.Paciente)
        {
            form.Controls.Add(UiFactory.CreateLabel("Paciente"), 0, row);
            form.Controls.Add(_cmbPacientes, 0, row + 1);
            form.SetColumnSpan(_cmbPacientes, 2);
            row += 2;
            _cmbPacientes.SelectedIndexChanged += async (_, _) =>
            {
                if (_cmbPacientes.SelectedValue is Guid patientId)
                {
                    _session.SelectedPatientId = patientId;
                }
                await LoadGridAsync();
            };
        }

        form.Controls.Add(UiFactory.CreateLabel("Nombre del medicamento"), 0, row);
        form.Controls.Add(UiFactory.CreateLabel("Dosis"), 1, row++);
        form.Controls.Add(_txtNombre, 0, row);
        form.Controls.Add(_txtDosis, 1, row++);
        form.Controls.Add(UiFactory.CreateLabel("Frecuencia"), 0, row);
        form.Controls.Add(UiFactory.CreateLabel("Horario"), 1, row++);
        form.Controls.Add(_txtFrecuencia, 0, row);
        form.Controls.Add(_txtHorario, 1, row++);
        form.Controls.Add(UiFactory.CreateLabel("Fecha de inicio"), 0, row);
        form.Controls.Add(UiFactory.CreateLabel("Fecha de fin"), 1, row++);
        form.Controls.Add(_dtpInicio, 0, row);
        form.Controls.Add(_dtpFin, 1, row++);
        form.Controls.Add(_chkActivo, 0, row++);
        form.Controls.Add(UiFactory.CreateLabel("Observaciones"), 0, row++);
        form.Controls.Add(_txtObservaciones, 0, row);
        form.SetColumnSpan(_txtObservaciones, 2);
        row++;

        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
        var btnSave = UiFactory.CreateButton("Guardar medicamento");
        btnSave.Click += async (_, _) => await SaveAsync();
        var btnDelete = UiFactory.CreateDangerButton("Eliminar seleccionado");
        btnDelete.Click += async (_, _) => await DeleteSelectedAsync();
        actions.Controls.Add(btnSave);
        actions.Controls.Add(btnDelete);
        form.Controls.Add(actions, 0, row);
        form.SetColumnSpan(actions, 2);

        formCard.Controls.Add(form);
        return formCard;
    }

    private Control BuildGridCard()
    {
        var gridCard = AppTheme.CreateCardPanel();
        gridCard.Padding = new Padding(20);
        var gridLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
        gridLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        gridLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        gridLayout.Controls.Add(UiFactory.CreateSectionTitle("Plan de medicación"), 0, 0);
        _grid.CellClick += (_, e) => LoadSelected(e.RowIndex);
        gridLayout.Controls.Add(_grid, 0, 1);
        gridCard.Controls.Add(gridLayout);
        return gridCard;
    }

    private async Task InitializeAsync()
    {
        if (_session.CurrentUser?.Rol != Core.Enums.UserRole.Paciente)
        {
            var patients = await PatientAccessHelper.GetAccessiblePatientsAsync(_services, _session);
            _cmbPacientes.DataSource = patients;
            _cmbPacientes.DisplayMember = "NombreCompleto";
            _cmbPacientes.ValueMember = "Id";
            await PatientAccessHelper.EnsureSelectedPatientAsync(_services, _session);
            PatientAccessHelper.ApplySelectedPatient(_cmbPacientes, _session);
            _lblHint.Text = "Selecciona un paciente, actualiza su tratamiento y revisa el historial activo.";
        }

        await LoadGridAsync();
    }

    private Guid CurrentPatientId => _session.CurrentUser?.Rol == Core.Enums.UserRole.Paciente
        ? _session.CurrentUser!.Id
        : _cmbPacientes.SelectedValue is Guid patientId ? patientId : Guid.Empty;

    private async Task SaveAsync()
    {
        if (CurrentPatientId == Guid.Empty)
        {
            MessageBox.Show("Selecciona un paciente válido antes de guardar.", "MediTrack");
            return;
        }

        var medication = new Medication
        {
            Id = _selectedMedicationId ?? Guid.Empty,
            PacienteId = CurrentPatientId,
            Nombre = _txtNombre.Text,
            Dosis = _txtDosis.Text,
            Frecuencia = _txtFrecuencia.Text,
            Horario = _txtHorario.Text,
            FechaInicio = _dtpInicio.Value.Date,
            FechaFin = _dtpFin.Value.Date,
            Observaciones = _txtObservaciones.Text,
            Activo = _chkActivo.Checked
        };

        var result = await _services.MedicationService.SaveAsync(medication);
        MessageBox.Show(result.Message, "MediTrack");
        if (result.Success)
        {
            ClearForm();
        }
        await LoadGridAsync();
    }

    private async Task DeleteSelectedAsync()
    {
        if (CurrentPatientId == Guid.Empty)
        {
            MessageBox.Show("Selecciona un paciente válido.", "MediTrack");
            return;
        }

        if (_grid.CurrentRow?.DataBoundItem is not null)
        {
            var id = (Guid)_grid.CurrentRow.Cells["Id"].Value;
            var result = await _services.MedicationService.DeleteAsync(id);
            MessageBox.Show(result.Message, "MediTrack");
            ClearForm();
            await LoadGridAsync();
        }
    }

    private async Task LoadGridAsync()
    {
        if (CurrentPatientId == Guid.Empty)
        {
            _items = [];
            _grid.DataSource = null;
            return;
        }

        _items = await _services.MedicationService.GetByPatientAsync(CurrentPatientId);
        _grid.DataSource = _items.Select(m => new
        {
            m.Id,
            m.Nombre,
            m.Dosis,
            m.Frecuencia,
            m.Horario,
            Inicio = m.FechaInicio.ToString("dd/MM/yyyy"),
            Fin = m.FechaFin?.ToString("dd/MM/yyyy") ?? "",
            m.Activo
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

        var medication = _items[rowIndex];
        _selectedMedicationId = medication.Id;
        _txtNombre.Text = medication.Nombre;
        _txtDosis.Text = medication.Dosis;
        _txtFrecuencia.Text = medication.Frecuencia;
        _txtHorario.Text = medication.Horario;
        _dtpInicio.Value = medication.FechaInicio;
        _dtpFin.Value = medication.FechaFin ?? DateTime.Today;
        _txtObservaciones.Text = medication.Observaciones;
        _chkActivo.Checked = medication.Activo;
    }

    private void ClearForm()
    {
        _selectedMedicationId = null;
        _txtNombre.Clear();
        _txtDosis.Clear();
        _txtFrecuencia.Clear();
        _txtHorario.Clear();
        _txtObservaciones.Clear();
        _dtpInicio.Value = DateTime.Today;
        _dtpFin.Value = DateTime.Today;
        _chkActivo.Checked = true;
    }
}
