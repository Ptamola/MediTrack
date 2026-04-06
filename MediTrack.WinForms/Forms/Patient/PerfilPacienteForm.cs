using MediTrack.WinForms.Config;
using MediTrack.WinForms.Forms.Shared;
using MediTrack.WinForms.Helpers;

namespace MediTrack.WinForms.Forms.Patient;

public class PerfilPacienteForm : BaseModuleForm
{
    private readonly ApplicationServices _services;
    private readonly AppSession _session;
    private readonly Label _lblNombre = UiFactory.CreateMutedLabel("Paciente");
    private readonly Label _lblEmail = UiFactory.CreateMutedLabel("Correo");
    private readonly Label _lblEdad = UiFactory.CreateMutedLabel("Edad");
    private readonly TextBox _txtTelefono = UiFactory.CreateTextBox();
    private readonly TextBox _txtDireccion = UiFactory.CreateTextBox();
    private readonly TextBox _txtObservaciones = UiFactory.CreateTextBox(multiline: true);
    private readonly DateTimePicker _dtpNacimiento = UiFactory.CreateDatePicker();

    public PerfilPacienteForm(ApplicationServices services, AppSession session) : base("Mi perfil")
    {
        _services = services;
        _session = session;

        var page = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            BackColor = AppTheme.Background
        };
        page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));
        page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68));

        var summaryCard = AppTheme.CreateMutedPanel();
        summaryCard.Padding = new Padding(24);
        var summaryLayout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = false
        };
        summaryLayout.Controls.Add(UiFactory.CreateSectionTitle("Resumen del paciente"));
        summaryLayout.Controls.Add(_lblNombre);
        summaryLayout.Controls.Add(_lblEmail);
        summaryLayout.Controls.Add(_lblEdad);
        summaryLayout.Controls.Add(UiFactory.CreateMutedLabel("Mantener el perfil actualizado ayuda a generar informes más claros y útiles."));
        summaryCard.Controls.Add(summaryLayout);

        var card = AppTheme.CreateCardPanel();
        card.Padding = new Padding(28);
        var form = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 5
        };
        form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        form.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        form.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        form.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        form.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        form.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        form.Controls.Add(UiFactory.CreateLabel("Fecha de nacimiento"), 0, 0);
        form.Controls.Add(UiFactory.CreateLabel("Teléfono"), 1, 0);
        form.Controls.Add(_dtpNacimiento, 0, 1);
        form.Controls.Add(_txtTelefono, 1, 1);
        form.Controls.Add(UiFactory.CreateLabel("Dirección"), 0, 2);
        form.Controls.Add(UiFactory.CreateLabel("Observaciones generales"), 1, 2);
        form.Controls.Add(_txtDireccion, 0, 3);
        form.Controls.Add(_txtObservaciones, 1, 3);

        var actionPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true
        };
        var btnSave = UiFactory.CreateButton("Guardar cambios");
        btnSave.Click += async (_, _) => await SaveAsync();
        actionPanel.Controls.Add(btnSave);
        form.Controls.Add(actionPanel, 0, 4);
        form.SetColumnSpan(actionPanel, 2);

        card.Controls.Add(form);
        page.Controls.Add(summaryCard, 0, 0);
        page.Controls.Add(card, 1, 0);
        ContentPanel.Controls.Add(page);

        Load += async (_, _) => await LoadProfileAsync();
    }

    private async Task LoadProfileAsync()
    {
        var patient = await _services.PatientService.GetByUserIdAsync(_session.CurrentUser!.Id);
        if (patient == null)
        {
            return;
        }

        var edad = DateTime.Today.Year - patient.FechaNacimiento.Year;
        if (patient.FechaNacimiento.Date > DateTime.Today.AddYears(-edad))
        {
            edad--;
        }

        _lblNombre.Text = $"Paciente: {_session.CurrentUser.NombreCompleto}";
        _lblEmail.Text = $"Correo: {_session.CurrentUser.Email}";
        _lblEdad.Text = $"Edad: {edad} años";
        _dtpNacimiento.Value = patient.FechaNacimiento;
        _txtTelefono.Text = patient.Telefono;
        _txtDireccion.Text = patient.Direccion;
        _txtObservaciones.Text = patient.ObservacionesGenerales;
    }

    private async Task SaveAsync()
    {
        var patient = new MediTrack.Core.Models.Patient
        {
            IdUsuario = _session.CurrentUser!.Id,
            FechaNacimiento = _dtpNacimiento.Value.Date,
            Telefono = _txtTelefono.Text,
            Direccion = _txtDireccion.Text,
            ObservacionesGenerales = _txtObservaciones.Text
        };

        var result = await _services.PatientService.UpdateAsync(patient);
        MessageBox.Show(result.Message, "MediTrack", MessageBoxButtons.OK,
            result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
    }
}
