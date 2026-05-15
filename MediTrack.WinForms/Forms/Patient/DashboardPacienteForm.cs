using MediTrack.Core.Helpers;
using MediTrack.WinForms.Config;
using MediTrack.WinForms.Forms.Shared;
using MediTrack.WinForms.Helpers;

namespace MediTrack.WinForms.Forms.Patient;

/// <summary>
/// Panel resumen del paciente. Agrupa enfermedades, medicacion, recordatorios,
/// mediciones recientes y notas visibles.
/// </summary>
public class DashboardPacienteForm : BaseModuleForm
{
    private readonly ApplicationServices _services;
    private readonly AppSession _session;
    private readonly ListBox _lstEnfermedades = UiFactory.CreateListBox();
    private readonly ListBox _lstRecordatorios = UiFactory.CreateListBox();
    private readonly DataGridView _gridMediciones = UiFactory.CreateGrid();
    private readonly DataGridView _gridNotas = UiFactory.CreateGrid();
    private readonly Label _lblCountEnfermedades = new();
    private readonly Label _lblCountMedicacion = new();
    private readonly Label _lblCountRecordatorios = new();
    private readonly Label _lblCountMediciones = new();

    public DashboardPacienteForm(ApplicationServices services, AppSession session) : base("Panel del paciente")
    {
        _services = services;
        _session = session;
        ConfigureGrids();

        var page = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            AutoScroll = true,
            BackColor = AppTheme.Background
        };
        page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        page.RowStyles.Add(new RowStyle(SizeType.Absolute, 130));
        page.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));
        page.RowStyles.Add(new RowStyle(SizeType.Percent, 34));
        page.RowStyles.Add(new RowStyle(SizeType.Percent, 66));

        var hero = BuildHero();
        var metrics = BuildMetrics();
        var mid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, AutoScroll = true };
        var bottom = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, AutoScroll = true };

        var diseasesCard = BuildListCard("Enfermedades crónicas", _lstEnfermedades);
        var remindersCard = BuildListCard("Próximas tomas", _lstRecordatorios);
        var measurementsCard = BuildGridCard("Últimas mediciones", _gridMediciones);
        var notesCard = BuildGridCard("Notas médicas visibles", _gridNotas);

        ApplyTwoPanelLayout(mid, diseasesCard, remindersCard, stacked: false, firstWeight: 50, secondWeight: 50);
        ApplyTwoPanelLayout(bottom, measurementsCard, notesCard, stacked: false, firstWeight: 54, secondWeight: 46);

        var stacked = false;
        page.Resize += (_, _) =>
        {
            var shouldStack = page.ClientSize.Width < 960;
            if (shouldStack == stacked)
            {
                return;
            }

            stacked = shouldStack;
            ApplyTwoPanelLayout(mid, diseasesCard, remindersCard, stacked, firstWeight: 50, secondWeight: 50);
            ApplyTwoPanelLayout(bottom, measurementsCard, notesCard, stacked, firstWeight: 54, secondWeight: 46);
            page.RowStyles[1] = new RowStyle(SizeType.Absolute, stacked ? 300 : 150);
            page.RowStyles[2] = stacked ? new RowStyle(SizeType.Absolute, 340) : new RowStyle(SizeType.Percent, 34);
            page.RowStyles[3] = stacked ? new RowStyle(SizeType.Absolute, 520) : new RowStyle(SizeType.Percent, 66);
        };

        page.Controls.Add(hero, 0, 0);
        page.Controls.Add(metrics, 0, 1);
        page.Controls.Add(mid, 0, 2);
        page.Controls.Add(bottom, 0, 3);
        ContentPanel.Controls.Add(page);

        Load += async (_, _) => await LoadDataAsync();
    }

    private Control BuildHero()
    {
        var hero = AppTheme.CreateCardPanel();
        hero.Padding = new Padding(24);
        hero.BackColor = Color.FromArgb(239, 246, 255);
        var heroLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = hero.BackColor };
        heroLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        heroLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        heroLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        heroLayout.Controls.Add(new Label
        {
            Text = $"Hola, {_session.CurrentUser?.Nombre}. Este es tu resumen clínico.",
            AutoSize = false,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI Semibold", 18, FontStyle.Bold),
            ForeColor = AppTheme.TextPrimary
        }, 0, 0);
        heroLayout.Controls.Add(UiFactory.CreateParagraphLabel("Consulta tus enfermedades, próximas tomas, últimas mediciones y notas médicas desde un solo lugar.", 52), 0, 1);
        hero.Controls.Add(heroLayout);
        return hero;
    }

    private Control BuildMetrics()
    {
        var metrics = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1, AutoScroll = true };
        metrics.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        for (var i = 0; i < 4; i++)
        {
            metrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        }

        metrics.Controls.Add(UiFactory.CreateMetricCard("Enfermedades activas", "0", "Patologías registradas", AppTheme.Primary), 0, 0);
        metrics.Controls.Add(UiFactory.CreateMetricCard("Medicación activa", "0", "Tratamientos actuales", AppTheme.Success), 1, 0);
        metrics.Controls.Add(UiFactory.CreateMetricCard("Próximas tomas", "0", "Recordatorios calculados", AppTheme.Warning), 2, 0);
        metrics.Controls.Add(UiFactory.CreateMetricCard("Últimas mediciones", "0", "Entradas recientes", AppTheme.Accent), 3, 0);

        _lblCountEnfermedades.Text = "0";
        _lblCountMedicacion.Text = "0";
        _lblCountRecordatorios.Text = "0";
        _lblCountMediciones.Text = "0";
        UpdateMetricValue(metrics.GetControlFromPosition(0, 0), _lblCountEnfermedades);
        UpdateMetricValue(metrics.GetControlFromPosition(1, 0), _lblCountMedicacion);
        UpdateMetricValue(metrics.GetControlFromPosition(2, 0), _lblCountRecordatorios);
        UpdateMetricValue(metrics.GetControlFromPosition(3, 0), _lblCountMediciones);
        return metrics;
    }

    private static void ApplyTwoPanelLayout(TableLayoutPanel layout, Control first, Control second, bool stacked, float firstWeight, float secondWeight)
    {
        layout.SuspendLayout();
        layout.Controls.Clear();
        layout.ColumnStyles.Clear();
        layout.RowStyles.Clear();

        if (stacked)
        {
            layout.ColumnCount = 1;
            layout.RowCount = 2;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            layout.Controls.Add(first, 0, 0);
            layout.Controls.Add(second, 0, 1);
        }
        else
        {
            layout.ColumnCount = 2;
            layout.RowCount = 1;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, firstWeight));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, secondWeight));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.Controls.Add(first, 0, 0);
            layout.Controls.Add(second, 1, 0);
        }

        layout.ResumeLayout();
    }

    private static void UpdateMetricValue(Control? card, Label source)
    {
        if (card == null)
        {
            return;
        }

        foreach (Control control in card.Controls)
        {
            if (control is TableLayoutPanel panel)
            {
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
    }

    private static Control BuildListCard(string title, ListBox list)
    {
        var card = AppTheme.CreateCardPanel();
        card.Padding = new Padding(18);
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(UiFactory.CreateSectionTitle(title), 0, 0);
        layout.Controls.Add(list, 0, 1);
        card.Controls.Add(layout);
        return card;
    }

    private static Control BuildGridCard(string title, DataGridView grid)
    {
        var card = AppTheme.CreateCardPanel();
        card.Padding = new Padding(18);
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(UiFactory.CreateSectionTitle(title), 0, 0);
        layout.Controls.Add(grid, 0, 1);
        card.Controls.Add(layout);
        return card;
    }

    /// <summary>
    /// Recarga las tarjetas y tablas del resumen usando los servicios de negocio.
    /// </summary>
    private async Task LoadDataAsync()
    {
        var patientId = _session.CurrentUser?.Id ?? Guid.Empty;
        var diseases = await _services.DiseaseService.GetPatientDiseasesAsync(patientId);
        var catalog = await _services.DiseaseService.GetCatalogAsync();
        var reminders = await _services.MedicationService.GetRemindersAsync(patientId);
        var medications = await _services.MedicationService.GetByPatientAsync(patientId);
        var latestMeasurements = await _services.MeasurementService.GetLatestAsync(patientId);
        var notes = await _services.MedicalNoteService.GetByPatientAsync(patientId, true);

        _lblCountEnfermedades.Text = diseases.Count.ToString();
        _lblCountMedicacion.Text = medications.Count(m => m.Activo).ToString();
        _lblCountRecordatorios.Text = reminders.Count.ToString();
        _lblCountMediciones.Text = latestMeasurements.Count.ToString();

        _lstEnfermedades.Items.Clear();
        foreach (var disease in diseases.Join(catalog, pd => pd.EnfermedadId, d => d.Id, (_, d) => d.Nombre))
        {
            _lstEnfermedades.Items.Add(disease);
        }

        _lstRecordatorios.Items.Clear();
        foreach (var reminder in reminders)
        {
            _lstRecordatorios.Items.Add($"{reminder.ProximaToma:dd/MM HH:mm} · {reminder.TextoRecordatorio}");
        }

        _gridMediciones.DataSource = latestMeasurements
            .Select(m => new
            {
                Fecha = m.FechaHora.ToString("dd/MM/yyyy HH:mm"),
                Tipo = MeasurementHelper.GetDisplayName(m.TipoMedicion),
                Valor = $"{m.Valor} {m.Unidad}"
            })
            .ToList();
        ApplyMeasurementsGridColumns();

        _gridNotas.DataSource = notes
            .Take(8)
            .Select(n => new { Fecha = n.FechaHora.ToString("dd/MM/yyyy"), n.Titulo, n.Contenido })
            .ToList();
        ApplyNotesGridColumns();
    }

    private void ConfigureGrids()
    {
        foreach (var grid in new[] { _gridMediciones, _gridNotas })
        {
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            grid.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            grid.RowTemplate.MinimumHeight = 42;
            grid.ScrollBars = ScrollBars.Vertical;
        }
    }

    private void ApplyMeasurementsGridColumns()
    {
        SetGridColumn(_gridMediciones, "Fecha", 30, 140, fixedWidth: true);
        SetGridColumn(_gridMediciones, "Tipo", 38, 150);
        SetGridColumn(_gridMediciones, "Valor", 24, 110);
    }

    private void ApplyNotesGridColumns()
    {
        SetGridColumn(_gridNotas, "Fecha", 22, 110, fixedWidth: true);
        SetGridColumn(_gridNotas, "Titulo", 28, 150, "Título");
        SetGridColumn(_gridNotas, "Contenido", 58, 260);
    }

    private static void SetGridColumn(DataGridView grid, string name, float fillWeight, int minimumWidth, string? header = null, bool fixedWidth = false)
    {
        if (grid.Columns[name] is not { } column)
        {
            return;
        }

        column.HeaderText = header ?? name;
        column.AutoSizeMode = fixedWidth ? DataGridViewAutoSizeColumnMode.None : DataGridViewAutoSizeColumnMode.Fill;
        column.FillWeight = fillWeight;
        column.MinimumWidth = minimumWidth;
        if (fixedWidth)
        {
            column.Width = minimumWidth;
            column.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
        }
        else
        {
            column.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        }
    }
}
