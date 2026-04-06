using MediTrack.Core.Helpers;
using MediTrack.WinForms.Config;
using MediTrack.WinForms.Forms.Shared;
using MediTrack.WinForms.Helpers;

namespace MediTrack.WinForms.Forms.Patient;

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

        var page = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4
        };
        page.RowStyles.Add(new RowStyle(SizeType.Absolute, 128));
        page.RowStyles.Add(new RowStyle(SizeType.Absolute, 130));
        page.RowStyles.Add(new RowStyle(SizeType.Absolute, 260));
        page.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var hero = AppTheme.CreateCardPanel();
        hero.Padding = new Padding(24);
        hero.BackColor = Color.FromArgb(239, 246, 255);
        var heroLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = hero.BackColor };
        heroLayout.Controls.Add(new Label
        {
            Text = $"Hola, {_session.CurrentUser?.Nombre}. Este es tu resumen clínico.",
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 18, FontStyle.Bold),
            ForeColor = AppTheme.TextPrimary
        }, 0, 0);
        heroLayout.Controls.Add(UiFactory.CreateMutedLabel("Consulta tus enfermedades, próximas tomas, últimas mediciones y notas médicas desde un solo lugar."), 0, 1);
        hero.Controls.Add(heroLayout);

        var metrics = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4 };
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

        var mid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
        mid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        mid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        mid.Controls.Add(BuildListCard("Enfermedades crónicas", _lstEnfermedades), 0, 0);
        mid.Controls.Add(BuildListCard("Próximas tomas", _lstRecordatorios), 1, 0);

        var bottom = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 54));
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46));
        bottom.Controls.Add(BuildGridCard("Últimas mediciones", _gridMediciones), 0, 0);
        bottom.Controls.Add(BuildGridCard("Notas médicas visibles", _gridNotas), 1, 0);

        page.Controls.Add(hero, 0, 0);
        page.Controls.Add(metrics, 0, 1);
        page.Controls.Add(mid, 0, 2);
        page.Controls.Add(bottom, 0, 3);
        ContentPanel.Controls.Add(page);

        Load += async (_, _) => await LoadDataAsync();
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
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
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
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(UiFactory.CreateSectionTitle(title), 0, 0);
        layout.Controls.Add(grid, 0, 1);
        card.Controls.Add(layout);
        return card;
    }

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

        _gridNotas.DataSource = notes
            .Take(8)
            .Select(n => new { Fecha = n.FechaHora.ToString("dd/MM/yyyy"), n.Titulo, n.Contenido })
            .ToList();
    }
}
