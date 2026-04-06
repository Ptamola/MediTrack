using MediTrack.Core.DTOs;
using MediTrack.Core.Enums;
using MediTrack.Core.Helpers;
using MediTrack.Core.Models;
using MediTrack.WinForms.Config;
using MediTrack.WinForms.Helpers;

namespace MediTrack.WinForms.Forms.Shared;

public class InformesForm : BaseModuleForm
{
    private readonly ApplicationServices _services;
    private readonly AppSession _session;
    private readonly ComboBox _cmbPacientes = UiFactory.CreateComboBox();
    private readonly DateTimePicker _dtpInicio = UiFactory.CreateDatePicker();
    private readonly DateTimePicker _dtpFin = UiFactory.CreateDatePicker();
    private readonly RichTextBox _preview = new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        BackColor = Color.White,
        BorderStyle = BorderStyle.None,
        DetectUrls = false,
        Font = new Font("Segoe UI", 11),
        ScrollBars = RichTextBoxScrollBars.Vertical
    };
    private readonly DataGridView _gridReports = UiFactory.CreateGrid();
    private readonly Label _lblStatus = UiFactory.CreateMutedLabel("Selecciona un paciente y un rango de fechas para generar el informe clínico.");
    private GeneratedReportResult? _lastGenerated;
    private string CurrentPatientName =>
        _session.CurrentUser?.Rol == UserRole.Paciente
            ? _session.CurrentUser?.NombreCompleto ?? "Paciente"
            : (_cmbPacientes.SelectedItem as User)?.NombreCompleto ?? "Paciente";

    public InformesForm(ApplicationServices services, AppSession session) : base("Informes clínicos")
    {
        _services = services;
        _session = session;

        var page = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2
        };
        page.RowStyles.Add(new RowStyle(SizeType.Absolute, 200));
        page.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        page.Controls.Add(BuildFilterCard(), 0, 0);
        page.Controls.Add(BuildContentLayout(), 0, 1);
        ContentPanel.Controls.Add(page);

        Load += async (_, _) => await InitializeAsync();
    }

    private Control BuildFilterCard()
    {
        var card = AppTheme.CreateCardPanel();
        card.Padding = new Padding(28, 24, 28, 24);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 4
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));

        layout.Controls.Add(UiFactory.CreateSectionTitle("Generación y exportación"), 0, 0);
        layout.SetColumnSpan(layout.GetControlFromPosition(0, 0)!, 4);

        var intro = UiFactory.CreateParagraphLabel("Genera informes clínicos, visualízalos dentro de la aplicación y expórtalos en PDF cuando lo necesites.", 48);
        layout.Controls.Add(intro, 0, 1);
        layout.SetColumnSpan(intro, 4);

        var patientPanel = BuildFieldPanel("Paciente", _cmbPacientes);
        var fromPanel = BuildFieldPanel("Desde", _dtpInicio);
        var toPanel = BuildFieldPanel("Hasta", _dtpFin);

        var actionsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoSize = true,
            Margin = new Padding(0, 8, 0, 0)
        };
        var btnGenerate = UiFactory.CreateButton("Generar informe");
        btnGenerate.Width = 190;
        btnGenerate.Click += async (_, _) => await GenerateAsync();
        var btnPdf = UiFactory.CreateButton("Exportar PDF", false);
        btnPdf.Width = 180;
        btnPdf.Click += async (_, _) => await ExportAsync();
        actionsPanel.Controls.Add(btnGenerate);
        actionsPanel.Controls.Add(btnPdf);

        if (_session.CurrentUser?.Rol != UserRole.Paciente)
        {
            layout.Controls.Add(patientPanel, 0, 2);
        }
        else
        {
            layout.Controls.Add(UiFactory.CreateInfoPanel("El informe se generará sobre tu propio perfil de paciente.", 74), 0, 2);
        }

        layout.Controls.Add(fromPanel, 1, 2);
        layout.Controls.Add(toPanel, 2, 2);
        layout.Controls.Add(actionsPanel, 3, 2);

        _lblStatus.MaximumSize = new Size(1200, 0);
        layout.Controls.Add(_lblStatus, 0, 3);
        layout.SetColumnSpan(_lblStatus, 4);

        card.Controls.Add(layout);
        return card;
    }

    private Control BuildContentLayout()
    {
        var content = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 56));
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44));

        var reportsCard = AppTheme.CreateCardPanel();
        reportsCard.Padding = new Padding(20);
        reportsCard.Margin = new Padding(0, 0, 10, 0);

        var reportsLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1
        };
        reportsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        reportsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        reportsLayout.Controls.Add(UiFactory.CreateSectionTitle("Historial de informes"), 0, 0);
        reportsLayout.Controls.Add(_gridReports, 0, 1);
        reportsCard.Controls.Add(reportsLayout);

        var previewCard = AppTheme.CreateCardPanel();
        previewCard.Padding = new Padding(24);
        previewCard.Margin = new Padding(10, 0, 0, 0);

        var previewLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1
        };
        previewLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        previewLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        previewLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        previewLayout.Controls.Add(UiFactory.CreateSectionTitle("Vista previa del informe"), 0, 0);
        previewLayout.Controls.Add(UiFactory.CreateMutedLabel("Resumen clínico legible dentro de la aplicación, listo para revisión antes de exportar."), 0, 1);

        var previewSurface = AppTheme.CreateMutedPanel();
        previewSurface.Padding = new Padding(22);
        previewSurface.Controls.Add(_preview);
        previewLayout.Controls.Add(previewSurface, 0, 2);
        previewCard.Controls.Add(previewLayout);

        content.Controls.Add(reportsCard, 0, 0);
        content.Controls.Add(previewCard, 1, 0);
        return content;
    }

    private static Panel BuildFieldPanel(string labelText, Control input)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 8, 16, 0),
            Padding = new Padding(0)
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
        _dtpInicio.Value = DateTime.Today.AddMonths(-1);
        _dtpFin.Value = DateTime.Today;

        _gridReports.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _gridReports.SelectionChanged += async (_, _) => await LoadSelectedReportPreviewAsync();

        if (_session.CurrentUser?.Rol != UserRole.Paciente)
        {
            var patients = await PatientAccessHelper.GetAccessiblePatientsAsync(_services, _session);
            _cmbPacientes.DataSource = patients;
            _cmbPacientes.DisplayMember = "NombreCompleto";
            _cmbPacientes.ValueMember = "Id";
            await PatientAccessHelper.EnsureSelectedPatientAsync(_services, _session);
            PatientAccessHelper.ApplySelectedPatient(_cmbPacientes, _session);
            _cmbPacientes.SelectedIndexChanged += async (_, _) =>
            {
                if (_cmbPacientes.SelectedValue is Guid patientId)
                {
                    _session.SelectedPatientId = patientId;
                }

                await ReloadReportsAsync();
            };
        }

        await ReloadReportsAsync();
    }

    private Guid CurrentPatientId => _session.CurrentUser?.Rol == UserRole.Paciente
        ? _session.CurrentUser!.Id
        : _cmbPacientes.SelectedValue is Guid patientId ? patientId : Guid.Empty;

    private async Task GenerateAsync()
    {
        if (CurrentPatientId == Guid.Empty)
        {
            MessageBox.Show("Selecciona un paciente válido.", "MediTrack");
            return;
        }

        if (_dtpInicio.Value.Date > _dtpFin.Value.Date)
        {
            MessageBox.Show("La fecha de inicio no puede ser posterior a la fecha de fin.", "MediTrack");
            return;
        }

        _lastGenerated = await _services.ReportService.GenerateAsync(
            CurrentPatientId,
            _session.CurrentUser!.Id,
            _dtpInicio.Value.Date,
            _dtpFin.Value.Date,
            true);

        RenderPreview(_lastGenerated);
        _lblStatus.Text = "Informe generado correctamente. Ya aparece en el historial y puede exportarse a PDF.";
        await ReloadReportsAsync(_lastGenerated.Reporte.Id);
    }

    private async Task ExportAsync()
    {
        if (_lastGenerated == null)
        {
            await GenerateAsync();
        }

        if (_lastGenerated == null)
        {
            return;
        }

        var suggestedName = BuildSuggestedFileName(_lastGenerated);
        using var dialog = new SaveFileDialog
        {
            Title = "Guardar informe PDF",
            Filter = "Archivo PDF (*.pdf)|*.pdf",
            DefaultExt = "pdf",
            AddExtension = true,
            OverwritePrompt = true,
            FileName = suggestedName,
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        };

        if (dialog.ShowDialog(this) != DialogResult.OK || string.IsNullOrWhiteSpace(dialog.FileName))
        {
            _lblStatus.Text = "Exportación cancelada por el usuario.";
            return;
        }

        var result = await _services.ReportService.ExportPdfAsync(_lastGenerated, dialog.FileName);
        MessageBox.Show(result.Message, "MediTrack", MessageBoxButtons.OK,
            result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        _lblStatus.Text = result.Message;
        await ReloadReportsAsync(_lastGenerated.Reporte.Id);
    }

    private static string BuildSuggestedFileName(GeneratedReportResult report)
    {
        var patientName = report.PacienteUsuario?.NombreCompleto ?? "Paciente";
        foreach (var invalid in Path.GetInvalidFileNameChars())
        {
            patientName = patientName.Replace(invalid, '_');
        }

        return $"Informe_{patientName}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
    }

    private async Task ReloadReportsAsync(Guid? selectedReportId = null)
    {
        if (CurrentPatientId == Guid.Empty)
        {
            _gridReports.DataSource = null;
            _preview.Clear();
            return;
        }

        var reports = await _services.ReportService.GetByPatientAsync(CurrentPatientId);
        _gridReports.DataSource = reports.Select(r => new
        {
            r.Id,
            Fecha = r.FechaGeneracion.ToString("dd/MM/yyyy HH:mm"),
            Periodo = $"{r.FechaInicioPeriodo:dd/MM/yyyy} - {r.FechaFinPeriodo:dd/MM/yyyy}",
            Paciente = CurrentPatientName,
            Pdf = string.IsNullOrWhiteSpace(r.RutaPdf) ? "Pendiente" : "Exportado"
        }).ToList();

        if (_gridReports.Columns.Contains("Id"))
        {
            _gridReports.Columns["Id"].Visible = false;
        }

        if (_gridReports.Columns.Contains("Fecha"))
        {
            _gridReports.Columns["Fecha"].FillWeight = 28;
            _gridReports.Columns["Periodo"].FillWeight = 34;
            _gridReports.Columns["Paciente"].FillWeight = 24;
            _gridReports.Columns["Pdf"].FillWeight = 14;
        }

        if (reports.Count == 0)
        {
            _preview.Clear();
            _preview.AppendText("Todavía no hay informes generados para este paciente.");
            return;
        }

        var targetId = selectedReportId ?? reports.First().Id;
        foreach (DataGridViewRow row in _gridReports.Rows)
        {
            if (row.Cells["Id"].Value is Guid id && id == targetId)
            {
                row.Selected = true;
                _gridReports.CurrentCell = row.Cells["Fecha"];
                break;
            }
        }
    }

    private async Task LoadSelectedReportPreviewAsync()
    {
        if (_gridReports.CurrentRow?.Cells["Id"].Value is not Guid reportId || CurrentPatientId == Guid.Empty)
        {
            return;
        }

        var reports = await _services.ReportService.GetByPatientAsync(CurrentPatientId);
        var report = reports.FirstOrDefault(r => r.Id == reportId);
        if (report == null)
        {
            return;
        }

        _lastGenerated = await _services.ReportService.GenerateAsync(
            CurrentPatientId,
            _session.CurrentUser!.Id,
            report.FechaInicioPeriodo,
            report.FechaFinPeriodo,
            false);
        _lastGenerated.Reporte.Id = report.Id;
        _lastGenerated.Reporte.RutaPdf = report.RutaPdf;
        _lastGenerated.Reporte.FechaGeneracion = report.FechaGeneracion;
        RenderPreview(_lastGenerated);
    }

    private void RenderPreview(GeneratedReportResult report)
    {
        _preview.Clear();
        _preview.SelectionIndent = 0;
        AppendTitle(report.Titulo);
        AppendMeta("Paciente", report.PacienteUsuario?.NombreCompleto ?? "No disponible");
        AppendMeta("Doctor asignado", report.DoctorAsignado?.NombreCompleto ?? "Sin asignar");
        AppendMeta("Doctores anteriores", report.DoctoresAnteriores.Count > 0 ? string.Join(", ", report.DoctoresAnteriores.Select(d => d.NombreCompleto)) : "No hay doctores previos registrados.");
        AppendMeta("Periodo", $"{report.Reporte.FechaInicioPeriodo:dd/MM/yyyy} - {report.Reporte.FechaFinPeriodo:dd/MM/yyyy}");
        AppendMeta("Estado PDF", string.IsNullOrWhiteSpace(report.Reporte.RutaPdf) ? "Pendiente de exportación" : "PDF exportado");

        AppendSection("Enfermedades activas", report.Enfermedades.Count > 0
            ? report.Enfermedades.Select(e => e.Nombre)
            : ["Sin enfermedades registradas."]);

        AppendSection("Medicacion actual", report.Medicamentos.Count > 0
            ? report.Medicamentos.Select(m => $"{m.Nombre} | {m.Dosis} | {m.Frecuencia} | {m.Horario}")
            : ["Sin medicación activa."]);

        AppendSection("Mediciones del periodo", report.Mediciones.Count > 0
            ? report.Mediciones
                .OrderByDescending(m => m.FechaHora)
                .Take(10)
                .Select(m => $"{m.FechaHora:dd/MM/yyyy HH:mm}  |  {MeasurementHelper.GetDisplayName(m.TipoMedicion)}  |  {m.Valor} {m.Unidad}")
            : ["Sin mediciones en el periodo seleccionado."]);

        AppendSection("Notas médicas", report.Notas.Count > 0
            ? report.Notas
                .OrderByDescending(n => n.FechaHora)
                .Take(8)
                .Select(n => $"{n.FechaHora:dd/MM/yyyy HH:mm}  |  {n.Titulo}: {n.Contenido}")
            : ["Sin notas médicas relevantes."]);

        AppendSummary(report.Reporte.Resumen);
    }

    private void AppendTitle(string text)
    {
        _preview.SelectionFont = new Font("Segoe UI Semibold", 16, FontStyle.Bold);
        _preview.SelectionColor = AppTheme.TextPrimary;
        _preview.AppendText(text + Environment.NewLine);
        _preview.SelectionFont = new Font("Segoe UI", 10);
        _preview.SelectionColor = AppTheme.TextSecondary;
        _preview.AppendText("Informe clínico generado dentro de MediTrack" + Environment.NewLine + Environment.NewLine);
    }

    private void AppendMeta(string label, string value)
    {
        _preview.SelectionFont = new Font("Segoe UI Semibold", 10, FontStyle.Bold);
        _preview.SelectionColor = AppTheme.TextPrimary;
        _preview.AppendText(label + ": ");
        _preview.SelectionFont = new Font("Segoe UI", 10);
        _preview.SelectionColor = AppTheme.TextSecondary;
        _preview.AppendText(value + Environment.NewLine);
    }

    private void AppendSection(string title, IEnumerable<string> lines)
    {
        _preview.AppendText(Environment.NewLine);
        _preview.SelectionFont = new Font("Segoe UI Semibold", 12, FontStyle.Bold);
        _preview.SelectionColor = AppTheme.TextPrimary;
        _preview.AppendText(title + Environment.NewLine);
        _preview.SelectionFont = new Font("Segoe UI", 10);
        _preview.SelectionColor = AppTheme.TextSecondary;
        foreach (var line in lines)
        {
            _preview.AppendText("• " + line + Environment.NewLine);
        }
    }

    private void AppendSummary(string text)
    {
        _preview.AppendText(Environment.NewLine);
        _preview.SelectionFont = new Font("Segoe UI Semibold", 12, FontStyle.Bold);
        _preview.SelectionColor = AppTheme.TextPrimary;
        _preview.AppendText("Resumen clínico" + Environment.NewLine);
        _preview.SelectionFont = new Font("Segoe UI", 10);
        _preview.SelectionColor = AppTheme.TextSecondary;
        _preview.AppendText(text + Environment.NewLine);
        _preview.SelectionStart = 0;
        _preview.SelectionLength = 0;
    }
}
