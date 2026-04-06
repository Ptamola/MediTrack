using MediTrack.WinForms.Config;
using MediTrack.WinForms.Forms.Shared;
using MediTrack.WinForms.Helpers;

namespace MediTrack.WinForms.Forms.Doctor;

public class DashboardDoctorForm : BaseModuleForm
{
    private readonly ApplicationServices _services;
    private readonly AppSession _session;
    private readonly DataGridView _grid = UiFactory.CreateGrid();
    private readonly Label _lblAssigned = new();
    private readonly Label _lblSelected = new();
    private readonly Label _lblWithDiseases = new();

    public DashboardDoctorForm(ApplicationServices services, AppSession session) : base("Dashboard del doctor")
    {
        _services = services;
        _session = session;

        var page = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
        page.RowStyles.Add(new RowStyle(SizeType.Absolute, 140));
        page.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var metrics = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3 };
        metrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
        metrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
        metrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
        metrics.Controls.Add(UiFactory.CreateMetricCard("Pacientes asignados", "0", "Bajo tu seguimiento", AppTheme.Primary), 0, 0);
        metrics.Controls.Add(UiFactory.CreateMetricCard("Paciente activo", "-", "Seleccionado para trabajar", AppTheme.Accent), 1, 0);
        metrics.Controls.Add(UiFactory.CreateMetricCard("Con enfermedades", "0", "Resumen del panel", AppTheme.Success), 2, 0);
        LinkMetric(metrics.GetControlFromPosition(0, 0), _lblAssigned);
        LinkMetric(metrics.GetControlFromPosition(1, 0), _lblSelected);
        LinkMetric(metrics.GetControlFromPosition(2, 0), _lblWithDiseases);

        var card = AppTheme.CreateCardPanel();
        card.Padding = new Padding(18);
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1 };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(UiFactory.CreateSectionTitle("Pacientes asignados"), 0, 0);
        layout.Controls.Add(UiFactory.CreateMutedLabel("Haz doble clic en una fila para establecer el paciente activo en los módulos clínicos."), 0, 1);
        layout.Controls.Add(_grid, 0, 2);
        card.Controls.Add(layout);

        _grid.CellDoubleClick += (_, e) =>
        {
            if (e.RowIndex >= 0)
            {
                _session.SelectedPatientId = (Guid)_grid.Rows[e.RowIndex].Cells["PacienteId"].Value;
                _lblSelected.Text = _grid.Rows[e.RowIndex].Cells["NombreCompleto"].Value?.ToString() ?? "-";
                MessageBox.Show("Paciente activo actualizado para los módulos clínicos.", "MediTrack");
            }
        };

        page.Controls.Add(metrics, 0, 0);
        page.Controls.Add(card, 0, 1);
        ContentPanel.Controls.Add(page);

        Load += async (_, _) => await LoadDataAsync();
    }

    private static void LinkMetric(Control? card, Label source)
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

    private async Task LoadDataAsync()
    {
        var items = await _services.PatientService.GetAssignedPatientsSummaryAsync(_session.CurrentUser!.Id);
        _grid.DataSource = items;
        if (_grid.Columns.Contains("PacienteId"))
        {
            _grid.Columns["PacienteId"].Visible = false;
        }

        _lblAssigned.Text = items.Count.ToString();
        _lblSelected.Text = items.FirstOrDefault(x => x.PacienteId == _session.SelectedPatientId)?.NombreCompleto ?? "-";
        _lblWithDiseases.Text = items.Count(x => !string.IsNullOrWhiteSpace(x.Enfermedades)).ToString();
    }
}
