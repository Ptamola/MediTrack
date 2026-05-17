using System.Drawing;
using System.Text;
using MediTrack.WinForms.Config;
using MediTrack.WinForms.Forms.Shared;
using MediTrack.WinForms.Helpers;

namespace MediTrack.WinForms.Forms.Doctor;

public class PacientesDoctorForm : BaseModuleForm
{
    private readonly ApplicationServices _services;
    private readonly AppSession _session;
    private readonly DataGridView _grid = UiFactory.CreateGrid();
    private readonly Label _lblInfo = UiFactory.CreateMutedLabel("Selecciona un paciente para usarlo en medicación, mediciones, notas e informes.");
    private readonly PictureBox _picPaciente = new()
    {
        Width = 150,
        Height = 150,
        SizeMode = PictureBoxSizeMode.Zoom,
        BorderStyle = BorderStyle.FixedSingle,
        BackColor = AppTheme.Surface
    };
    private readonly RichTextBox _txtDetallePaciente = new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        BorderStyle = BorderStyle.None,
        BackColor = AppTheme.Surface,
        Font = AppTheme.BodyFont,
        ForeColor = AppTheme.TextPrimary
    };

    public PacientesDoctorForm(ApplicationServices services, AppSession session) : base("Pacientes asignados")
    {
        _services = services;
        _session = session;
        ConfigureGrid();

        var page = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1, AutoScroll = true };
        page.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));
        page.RowStyles.Add(new RowStyle(SizeType.Percent, 58));
        page.RowStyles.Add(new RowStyle(SizeType.Absolute, 260));

        var headerCard = AppTheme.CreateMutedPanel();
        headerCard.Padding = new Padding(24);
        var headerLayout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true
        };
        headerLayout.Controls.Add(UiFactory.CreateSectionTitle("Selección de contexto clínico"));
        headerLayout.Controls.Add(_lblInfo);
        var btnSelect = UiFactory.CreateButton("Usar paciente seleccionado");
        btnSelect.Click += (_, _) => SelectCurrent();
        headerLayout.Controls.Add(btnSelect);
        headerCard.Controls.Add(headerLayout);

        var listCard = BuildListCard();
        var detailCard = BuildDetailCard();

        page.Controls.Add(headerCard, 0, 0);
        page.Controls.Add(listCard, 0, 1);
        page.Controls.Add(detailCard, 0, 2);
        ContentPanel.Controls.Add(page);

        Load += async (_, _) => await LoadDataAsync();
    }

    private Control BuildListCard()
    {
        var card = AppTheme.CreateCardPanel();
        card.Padding = new Padding(20);
        var contentLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
        contentLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        contentLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        contentLayout.Controls.Add(UiFactory.CreateSectionTitle("Listado de pacientes"), 0, 0);
        contentLayout.Controls.Add(_grid, 0, 1);
        card.Controls.Add(contentLayout);
        return card;
    }

    private Control BuildDetailCard()
    {
        var card = AppTheme.CreateCardPanel();
        card.Padding = new Padding(20);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            BackColor = AppTheme.Surface
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var title = UiFactory.CreateSectionTitle("Ficha ampliada del paciente");
        layout.Controls.Add(title, 0, 0);
        layout.SetColumnSpan(title, 2);
        layout.Controls.Add(_picPaciente, 0, 1);
        layout.Controls.Add(_txtDetallePaciente, 1, 1);
        card.Controls.Add(layout);
        return card;
    }

    private async Task LoadDataAsync()
    {
        var items = await _services.PatientService.GetAssignedPatientsSummaryAsync(_session.CurrentUser!.Id);
        _grid.DataSource = items;
        if (_grid.Columns.Contains("PacienteId"))
        {
            _grid.Columns["PacienteId"].Visible = false;
        }

        ApplyGridColumns();
        _grid.SelectionChanged -= GridSelectionChanged;
        _grid.SelectionChanged += GridSelectionChanged;
        await LoadSelectedPatientDetailsAsync();
    }

    private void SelectCurrent()
    {
        if (_grid.CurrentRow?.Cells["PacienteId"].Value is Guid patientId)
        {
            _session.SelectedPatientId = patientId;
            MessageBox.Show("Paciente seleccionado para trabajar en el resto de módulos.", "MediTrack");
        }
    }

    private void GridSelectionChanged(object? sender, EventArgs e)
    {
        _ = LoadSelectedPatientDetailsAsync();
    }

    private async Task LoadSelectedPatientDetailsAsync()
    {
        if (_grid.CurrentRow?.Cells["PacienteId"].Value is not Guid patientId)
        {
            _txtDetallePaciente.Text = "Selecciona un paciente para ver su ficha ampliada.";
            ClearPhoto();
            return;
        }

        var patient = await _services.PatientService.GetByUserIdAsync(patientId);
        if (patient == null)
        {
            _txtDetallePaciente.Text = "No se ha encontrado el perfil del paciente.";
            ClearPhoto();
            return;
        }

        var nombre = _grid.CurrentRow.Cells["NombreCompleto"]?.Value?.ToString() ?? "Paciente";
        var email = _grid.CurrentRow.Cells["Email"]?.Value?.ToString() ?? string.Empty;
        var enfermedades = _grid.CurrentRow.Cells["Enfermedades"]?.Value?.ToString() ?? "Sin enfermedades registradas";

        var detalle = new StringBuilder();
        detalle.AppendLine(nombre);
        detalle.AppendLine($"Email: {email}");
        detalle.AppendLine($"DNI/NIE: {ValueOrDash(patient.DniNie)}");
        detalle.AppendLine($"Fecha de nacimiento: {patient.FechaNacimiento:dd/MM/yyyy}");
        detalle.AppendLine($"Sexo: {ValueOrDash(patient.Sexo)}");
        detalle.AppendLine($"Grupo sanguíneo: {ValueOrDash(patient.GrupoSanguineo)}");
        detalle.AppendLine($"Altura: {FormatDecimal(patient.AlturaCm, "cm")}");
        detalle.AppendLine($"Peso: {FormatDecimal(patient.PesoKg, "kg")}");
        detalle.AppendLine($"Teléfono: {ValueOrDash(patient.Telefono)}");
        detalle.AppendLine($"Dirección: {ValueOrDash(patient.Direccion)}");
        detalle.AppendLine($"Contacto de emergencia: {ValueOrDash(patient.ContactoEmergenciaNombre)}");
        detalle.AppendLine($"Teléfono de emergencia: {ValueOrDash(patient.ContactoEmergenciaTelefono)}");
        detalle.AppendLine($"Seguro médico: {ValueOrDash(patient.SeguroMedico)}");
        detalle.AppendLine($"Tarjeta sanitaria: {ValueOrDash(patient.NumeroTarjetaSanitaria)}");
        detalle.AppendLine($"Enfermedades: {ValueOrDash(enfermedades)}");
        detalle.AppendLine();
        detalle.AppendLine($"Alergias: {ValueOrDash(patient.Alergias)}");
        detalle.AppendLine($"Antecedentes médicos: {ValueOrDash(patient.AntecedentesMedicos)}");
        detalle.AppendLine($"Información extra: {ValueOrDash(patient.ObservacionesGenerales)}");
        _txtDetallePaciente.Text = detalle.ToString();
        LoadPhoto(patient.FotoRuta);
    }

    private void LoadPhoto(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            ClearPhoto();
            return;
        }

        var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", relativePath);
        if (!File.Exists(fullPath))
        {
            ClearPhoto();
            return;
        }

        var previous = _picPaciente.Image;
        using var image = Image.FromFile(fullPath);
        _picPaciente.Image = new Bitmap(image);
        previous?.Dispose();
    }

    private void ClearPhoto()
    {
        var previous = _picPaciente.Image;
        _picPaciente.Image = null;
        previous?.Dispose();
    }

    private void ConfigureGrid()
    {
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        _grid.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        _grid.RowTemplate.MinimumHeight = 44;
        _grid.ScrollBars = ScrollBars.Vertical;
    }

    private void ApplyGridColumns()
    {
        SetFillColumn("NombreCompleto", "Paciente", 28, 160);
        SetFillColumn("Email", "Email", 24, 165);
        SetFillColumn("Telefono", "Teléfono", 14, 100);
        SetFillColumn("Enfermedades", "Enfermedades", 46, 220);
    }

    private void SetFillColumn(string name, string header, float fillWeight, int minimumWidth)
    {
        if (_grid.Columns[name] is not { } column)
        {
            return;
        }

        column.HeaderText = header;
        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        column.FillWeight = fillWeight;
        column.MinimumWidth = minimumWidth;
        column.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
    }

    private static string ValueOrDash(string value) => string.IsNullOrWhiteSpace(value) ? "No indicado" : value.Trim();

    private static string FormatDecimal(decimal value, string unit) => value <= 0 ? "No indicado" : $"{value:0.#} {unit}";
}
