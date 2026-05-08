using MediTrack.Core.Enums;
using MediTrack.Core.Helpers;
using MediTrack.Core.Models;
using MediTrack.WinForms.Config;
using MediTrack.WinForms.Forms.Shared;
using MediTrack.WinForms.Helpers;

namespace MediTrack.WinForms.Forms.Patient;

public class MedicionesForm : BaseModuleForm
{
    private readonly ApplicationServices _services;
    private readonly AppSession _session;
    private readonly ComboBox _cmbPacientes = UiFactory.CreateComboBox();
    private readonly ComboBox _cmbTipo = UiFactory.CreateComboBox();
    private readonly TextBox _txtValor = UiFactory.CreateTextBox();
    private readonly TextBox _txtObservaciones = UiFactory.CreateTextBox(multiline: true);
    private readonly DataGridView _grid = UiFactory.CreateGrid();
    private readonly DateTimePicker _dtpFiltroInicio = UiFactory.CreateDatePicker();
    private readonly DateTimePicker _dtpFiltroFin = UiFactory.CreateDatePicker();
    private readonly MeasurementsChartPanel _chart = new() { Dock = DockStyle.Fill };
    private readonly Label _lblHint = UiFactory.CreateParagraphLabel("Registra tus mediciones, filtra por fechas y revisa tu evolución en tiempo real.");

    public MedicionesForm(ApplicationServices services, AppSession session) : base("Mediciones clínicas")
    {
        _services = services;
        _session = session;
        ConfigureGrid();

        var page = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            AutoScroll = true,
            BackColor = AppTheme.Background
        };

        var formCard = BuildFormCard();
        var gridCard = BuildGridCard();
        var chartCard = BuildChartCard();

        var stacked = false;
        ApplyResponsiveLayout(page, formCard, gridCard, chartCard, stacked);
        page.Resize += (_, _) =>
        {
            var shouldStack = page.ClientSize.Width < 980;
            if (shouldStack == stacked)
            {
                return;
            }

            stacked = shouldStack;
            ApplyResponsiveLayout(page, formCard, gridCard, chartCard, stacked);
        };

        ContentPanel.Controls.Add(page);
        Load += async (_, _) => await InitializeAsync();
    }

    private static void ApplyResponsiveLayout(TableLayoutPanel page, Control formCard, Control gridCard, Control chartCard, bool stacked)
    {
        page.SuspendLayout();
        page.Controls.Clear();
        page.ColumnStyles.Clear();
        page.RowStyles.Clear();

        if (stacked)
        {
            page.ColumnCount = 1;
            page.RowCount = 3;
            page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 620));
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 430));
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 380));
            page.Controls.Add(formCard, 0, 0);
            page.Controls.Add(gridCard, 0, 1);
            page.Controls.Add(chartCard, 0, 2);
        }
        else
        {
            page.ColumnCount = 2;
            page.RowCount = 2;
            page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
            page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 66));
            page.RowStyles.Add(new RowStyle(SizeType.Percent, 58));
            page.RowStyles.Add(new RowStyle(SizeType.Percent, 42));
            page.Controls.Add(formCard, 0, 0);
            page.SetRowSpan(formCard, 2);
            page.Controls.Add(gridCard, 1, 0);
            page.Controls.Add(chartCard, 1, 1);
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
        _txtObservaciones.Height = 120;

        form.Controls.Add(UiFactory.CreateSectionTitle("Nueva medición"), 0, 0);
        form.Controls.Add(_lblHint, 0, 1);
        var row = 2;

        if (_session.CurrentUser?.Rol != UserRole.Paciente)
        {
            form.Controls.Add(UiFactory.CreateLabel("Paciente"), 0, row++);
            form.Controls.Add(_cmbPacientes, 0, row++);
            _cmbPacientes.SelectedIndexChanged += async (_, _) =>
            {
                if (_cmbPacientes.SelectedValue is Guid patientId)
                {
                    _session.SelectedPatientId = patientId;
                }
                await ReloadAsync();
            };
        }

        form.Controls.Add(UiFactory.CreateLabel("Tipo de medición"), 0, row++);
        form.Controls.Add(_cmbTipo, 0, row++);
        form.Controls.Add(UiFactory.CreateLabel("Valor"), 0, row++);
        form.Controls.Add(_txtValor, 0, row++);
        form.Controls.Add(UiFactory.CreateLabel("Observaciones o síntomas"), 0, row++);
        form.Controls.Add(_txtObservaciones, 0, row++);

        var info = UiFactory.CreateInfoPanel("Consejo: registra glucosa, presión, peso y síntomas para generar informes más completos.");
        form.Controls.Add(info, 0, row++);

        var btnAdd = UiFactory.CreateButton("Registrar medición");
        btnAdd.Click += async (_, _) => await SaveAsync();
        form.Controls.Add(btnAdd, 0, row++);

        left.Controls.Add(form);
        return left;
    }

    private Control BuildGridCard()
    {
        var gridCard = AppTheme.CreateCardPanel();
        gridCard.Padding = new Padding(20);
        var gridLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1 };
        gridLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        gridLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        gridLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        gridLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var filterPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            Margin = new Padding(0, 0, 0, 12)
        };

        var btnFilter = UiFactory.CreateButton("Filtrar", false);
        btnFilter.Width = 140;
        btnFilter.Click += async (_, _) => await ReloadAsync();
        filterPanel.Controls.Add(UiFactory.CreateLabel("Desde"));
        filterPanel.Controls.Add(_dtpFiltroInicio);
        filterPanel.Controls.Add(UiFactory.CreateLabel("Hasta"));
        filterPanel.Controls.Add(_dtpFiltroFin);
        filterPanel.Controls.Add(btnFilter);

        gridLayout.Controls.Add(UiFactory.CreateSectionTitle("Historial de mediciones"), 0, 0);
        gridLayout.Controls.Add(filterPanel, 0, 1);
        gridLayout.Controls.Add(_grid, 0, 2);
        gridCard.Controls.Add(gridLayout);
        return gridCard;
    }

    private Control BuildChartCard()
    {
        var chartCard = AppTheme.CreateCardPanel();
        chartCard.Padding = new Padding(20);
        var chartLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
        chartLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        chartLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        chartLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        chartLayout.Controls.Add(UiFactory.CreateSectionTitle("Evolución visual"), 0, 0);
        chartLayout.Controls.Add(_chart, 0, 1);
        chartCard.Controls.Add(chartLayout);
        return chartCard;
    }

    private async Task InitializeAsync()
    {
        _cmbTipo.DataSource = Enum.GetValues(typeof(MeasurementType));
        _dtpFiltroInicio.Value = DateTime.Today.AddMonths(-1);
        _dtpFiltroFin.Value = DateTime.Today;
        if (_session.CurrentUser?.Rol != UserRole.Paciente)
        {
            var patients = await PatientAccessHelper.GetAccessiblePatientsAsync(_services, _session);
            _cmbPacientes.DataSource = patients;
            _cmbPacientes.DisplayMember = "NombreCompleto";
            _cmbPacientes.ValueMember = "Id";
            await PatientAccessHelper.EnsureSelectedPatientAsync(_services, _session);
            PatientAccessHelper.ApplySelectedPatient(_cmbPacientes, _session);
            _lblHint.Text = "Selecciona un paciente para registrar valores y revisar tendencias de seguimiento.";
        }

        await ReloadAsync();
    }

    private Guid CurrentPatientId => _session.CurrentUser?.Rol == UserRole.Paciente
        ? _session.CurrentUser!.Id
        : _cmbPacientes.SelectedValue is Guid patientId ? patientId : Guid.Empty;

    private async Task SaveAsync()
    {
        if (CurrentPatientId == Guid.Empty)
        {
            MessageBox.Show("Selecciona un paciente válido.", "MediTrack");
            return;
        }

        if (_cmbTipo.SelectedItem is not MeasurementType type)
        {
            MessageBox.Show("Selecciona un tipo de medición válido.", "MediTrack");
            return;
        }

        decimal.TryParse(_txtValor.Text.Replace('.', ','), out var value);
        var measurement = new Measurement
        {
            PacienteId = CurrentPatientId,
            TipoMedicion = type,
            Valor = value,
            Observaciones = _txtObservaciones.Text,
            FechaHora = DateTime.Now
        };

        var result = await _services.MeasurementService.SaveAsync(measurement);
        MessageBox.Show(result.Message, "MediTrack");
        _txtValor.Clear();
        _txtObservaciones.Clear();
        await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        if (CurrentPatientId == Guid.Empty)
        {
            _grid.DataSource = null;
            _chart.Measurements = [];
            _chart.Invalidate();
            return;
        }

        var items = await _services.MeasurementService.GetByPatientAsync(
            CurrentPatientId,
            null,
            _dtpFiltroInicio.Value.Date,
            _dtpFiltroFin.Value.Date.AddDays(1).AddSeconds(-1));

        _grid.DataSource = items.Select(m => new
        {
            Fecha = m.FechaHora.ToString("dd/MM/yyyy HH:mm"),
            Tipo = MeasurementHelper.GetDisplayName(m.TipoMedicion),
            Valor = $"{m.Valor} {m.Unidad}",
            m.Observaciones
        }).ToList();
        ApplyGridColumns();

        _chart.Measurements = items;
        _chart.Invalidate();
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
        if (_grid.Columns["Fecha"] is { } fecha)
        {
            fecha.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            fecha.Width = 145;
            fecha.MinimumWidth = 130;
            fecha.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
        }

        if (_grid.Columns["Tipo"] is { } tipo)
        {
            tipo.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            tipo.FillWeight = 22;
            tipo.MinimumWidth = 140;
        }

        if (_grid.Columns["Valor"] is { } valor)
        {
            valor.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            valor.FillWeight = 18;
            valor.MinimumWidth = 110;
        }

        if (_grid.Columns["Observaciones"] is { } observaciones)
        {
            observaciones.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            observaciones.FillWeight = 45;
            observaciones.MinimumWidth = 220;
            observaciones.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        }
    }
}
