using MediTrack.Core.Models;
using MediTrack.WinForms.Config;
using MediTrack.WinForms.Forms.Shared;
using MediTrack.WinForms.Helpers;

namespace MediTrack.WinForms.Forms.Patient;

/// <summary>
/// Pantalla de enfermedades cronicas. Permite al doctor asignar enfermedades existentes
/// o escribir una nueva, y marcar relaciones como activas o superadas.
/// </summary>
public class EnfermedadesForm : BaseModuleForm
{
    private readonly ApplicationServices _services;
    private readonly AppSession _session;
    private readonly ComboBox _cmbPacientes = UiFactory.CreateComboBox();
    private readonly ComboBox _cmbEnfermedades = UiFactory.CreateComboBox();
    private readonly DateTimePicker _dtpDiagnostico = UiFactory.CreateDatePicker();
    private readonly ComboBox _cmbEstado = UiFactory.CreateComboBox();
    private readonly DateTimePicker _dtpFechaFin = UiFactory.CreateDatePicker();
    private readonly TextBox _txtObservaciones = UiFactory.CreateTextBox(multiline: true);
    private readonly DataGridView _grid = UiFactory.CreateGrid();
    private readonly Label _lblResumen = UiFactory.CreateParagraphLabel("Consulta tus enfermedades activas y mantén actualizadas las observaciones clínicas.");
    private readonly ToolTip _toolTip = new();
    private List<PatientDisease> _items = [];
    private Guid? _selectedRelationId;

    public EnfermedadesForm(ApplicationServices services, AppSession session) : base("Enfermedades crónicas")
    {
        _services = services;
        _session = session;
        ConfigureGrid();
        ConfigureDiseaseSelector();
        ConfigureStatusSelector();

        var page = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            AutoScroll = true,
            BackColor = AppTheme.Background
        };

        var left = BuildFormCard();
        var right = BuildGridCard();

        var stacked = false;
        ApplyResponsiveLayout(page, left, right, stacked);
        page.Resize += (_, _) =>
        {
            var shouldStack = page.ClientSize.Width < 1060;
            if (shouldStack == stacked)
            {
                return;
            }

            stacked = shouldStack;
            ApplyResponsiveLayout(page, left, right, stacked);
        };

        ContentPanel.Controls.Add(page);

        Load += async (_, _) => await InitializeAsync();
    }

    private static void ApplyResponsiveLayout(TableLayoutPanel page, Control formCard, Control gridCard, bool stacked)
    {
        page.SuspendLayout();
        page.Controls.Clear();
        page.ColumnStyles.Clear();
        page.RowStyles.Clear();

        if (stacked)
        {
            page.ColumnCount = 1;
            page.RowCount = 2;
            page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 590));
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 430));
            page.Controls.Add(formCard, 0, 0);
            page.Controls.Add(gridCard, 0, 1);
        }
        else
        {
            page.ColumnCount = 2;
            page.RowCount = 1;
            page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
            page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 66));
            page.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            page.Controls.Add(formCard, 0, 0);
            page.Controls.Add(gridCard, 1, 0);
        }

        page.ResumeLayout();
    }

    private Control BuildFormCard()
    {
        var left = AppTheme.CreateCardPanel();
        left.Padding = new Padding(24);
        var form = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            AutoScroll = true,
            BackColor = AppTheme.Surface
        };
        form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        _txtObservaciones.Height = 130;

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
        form.Controls.Add(UiFactory.CreateLabel("Estado"), 0, row++);
        form.Controls.Add(_cmbEstado, 0, row++);
        form.Controls.Add(UiFactory.CreateLabel("Fecha de resolución"), 0, row++);
        form.Controls.Add(_dtpFechaFin, 0, row++);
        form.Controls.Add(UiFactory.CreateLabel("Observaciones"), 0, row++);
        form.Controls.Add(_txtObservaciones, 0, row++);

        var note = UiFactory.CreateInfoPanel("Puedes seleccionar una enfermedad del historial para actualizar el diagnóstico u observaciones.");
        form.Controls.Add(note, 0, row++);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            WrapContents = true,
            FlowDirection = FlowDirection.LeftToRight,
            Margin = new Padding(0, 8, 0, 0)
        };
        var btnAdd = UiFactory.CreateButton("Añadir enfermedad");
        btnAdd.Click += async (_, _) => await AddAsync();
        _toolTip.SetToolTip(btnAdd, "Asigna al paciente una enfermedad nueva o una enfermedad existente del catálogo.");

        var btnUpdate = UiFactory.CreateButton("Guardar edición", false);
        btnUpdate.Click += async (_, _) => await UpdateAsync();
        _toolTip.SetToolTip(btnUpdate, "Guarda los cambios de fecha, estado u observaciones de la enfermedad seleccionada.");
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
        rightLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        rightLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        rightLayout.Controls.Add(UiFactory.CreateSectionTitle("Historial de enfermedades"), 0, 0);
        _grid.CellClick += (_, e) => LoadSelected(e.RowIndex);
        rightLayout.Controls.Add(_grid, 0, 1);
        right.Controls.Add(rightLayout);
        return right;
    }

    /// <summary>
    /// Prepara catalogo, pacientes accesibles y listado inicial.
    /// </summary>
    private async Task InitializeAsync()
    {
        await LoadDiseaseCatalogAsync();

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

    /// <summary>
    /// Asigna una enfermedad al paciente; si el texto no existe en catalogo, el servicio la crea.
    /// </summary>
    private async Task AddAsync()
    {
        if (GetPatientId() == Guid.Empty)
        {
            MessageBox.Show("Selecciona un paciente válido.", "MediTrack");
            return;
        }

        var diseaseName = _cmbEnfermedades.Text.Trim();
        if (string.IsNullOrWhiteSpace(diseaseName))
        {
            MessageBox.Show("Selecciona o escribe una enfermedad válida.", "MediTrack");
            return;
        }

        var result = await _services.DiseaseService.AssignDiseaseByNameAsync(
            GetPatientId(),
            diseaseName,
            _dtpDiagnostico.Value.Date,
            _txtObservaciones.Text);

        MessageBox.Show(result.Message, "MediTrack");
        if (result.Success)
        {
            ClearSelection();
            await LoadDiseaseCatalogAsync();
        }
        await LoadGridAsync();
    }

    /// <summary>
    /// Actualiza fecha, observaciones y estado activo/superado de una enfermedad asignada.
    /// </summary>
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
        existing.Activa = _cmbEstado.SelectedIndex != 1;
        existing.FechaFin = existing.Activa ? null : _dtpFechaFin.Value.Date;
        if (!existing.Activa && _dtpFechaFin.Value.Date < existing.FechaDiagnostico.Date)
        {
            MessageBox.Show("La fecha de resolución no puede ser anterior a la fecha de diagnóstico.", "MediTrack");
            return;
        }

        existing.Observaciones = _txtObservaciones.Text;
        var result = await _services.DiseaseService.UpdateAsync(existing);
        MessageBox.Show(result.Message, "MediTrack");
        if (result.Success)
        {
            ClearSelection();
        }
        await LoadGridAsync();
    }

    /// <summary>
    /// Carga el historial de enfermedades del paciente seleccionado con estado y fecha de fin.
    /// </summary>
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
            FechaDiagnostico = pd.FechaDiagnostico.ToString("dd/MM/yyyy"),
            FechaFin = pd.FechaFin?.ToString("dd/MM/yyyy") ?? "-",
            Estado = pd.Activa ? "Activa" : "Superada",
            pd.Observaciones
        }).ToList();

        ApplyGridColumns();
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
        _cmbEstado.SelectedIndex = selected.Activa ? 0 : 1;
        _dtpFechaFin.Value = selected.FechaFin ?? DateTime.Today;
        _dtpFechaFin.Enabled = !selected.Activa;
        _txtObservaciones.Text = selected.Observaciones;
    }

    private void ClearSelection()
    {
        _selectedRelationId = null;
        _txtObservaciones.Clear();
        _dtpDiagnostico.Value = DateTime.Today;
        _cmbEstado.SelectedIndex = 0;
        _dtpFechaFin.Value = DateTime.Today;
        _dtpFechaFin.Enabled = false;
    }

    private async Task LoadDiseaseCatalogAsync()
    {
        var diseases = await _services.DiseaseService.GetCatalogAsync();
        _cmbEnfermedades.DataSource = diseases;
        _cmbEnfermedades.DisplayMember = "Nombre";
        _cmbEnfermedades.ValueMember = "Id";
    }

    private void ConfigureDiseaseSelector()
    {
        _cmbEnfermedades.DropDownStyle = ComboBoxStyle.DropDown;
        _cmbEnfermedades.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        _cmbEnfermedades.AutoCompleteSource = AutoCompleteSource.ListItems;
    }

    private void ConfigureStatusSelector()
    {
        _cmbEstado.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbEstado.Items.AddRange(new object[] { "Activa", "Superada/Inactiva" });
        _cmbEstado.SelectedIndex = 0;
        _dtpFechaFin.Enabled = false;
        _cmbEstado.SelectedIndexChanged += (_, _) =>
        {
            _dtpFechaFin.Enabled = _cmbEstado.SelectedIndex == 1;
        };
    }

    private void ConfigureGrid()
    {
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        _grid.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        _grid.RowTemplate.MinimumHeight = 42;
        _grid.ScrollBars = ScrollBars.Both;
    }

    private void ApplyGridColumns()
    {
        if (_grid.Columns.Contains("Id"))
        {
            _grid.Columns["Id"].Visible = false;
        }

        if (_grid.Columns["Enfermedad"] is { } enfermedad)
        {
            enfermedad.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            enfermedad.FillWeight = 28;
            enfermedad.MinimumWidth = 150;
        }

        if (_grid.Columns["FechaDiagnostico"] is { } fecha)
        {
            fecha.HeaderText = "Fecha diagnóstico";
            fecha.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            fecha.Width = 130;
            fecha.MinimumWidth = 120;
            fecha.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
        }

        if (_grid.Columns["FechaFin"] is { } fechaFin)
        {
            fechaFin.HeaderText = "Fecha fin";
            fechaFin.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            fechaFin.Width = 105;
            fechaFin.MinimumWidth = 95;
            fechaFin.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
        }

        if (_grid.Columns["Estado"] is { } estado)
        {
            estado.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            estado.Width = 120;
            estado.MinimumWidth = 105;
            estado.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
        }

        if (_grid.Columns["Observaciones"] is { } observaciones)
        {
            observaciones.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            observaciones.FillWeight = 52;
            observaciones.MinimumWidth = 210;
            observaciones.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        }
    }
}
