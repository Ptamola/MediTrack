using System.Drawing;
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
    private readonly PictureBox _picFoto = new()
    {
        Width = 180,
        Height = 180,
        SizeMode = PictureBoxSizeMode.Zoom,
        BorderStyle = BorderStyle.FixedSingle,
        BackColor = AppTheme.Surface
    };

    private readonly TextBox _txtDniNie = UiFactory.CreateTextBox();
    private readonly TextBox _txtTelefono = UiFactory.CreateTextBox();
    private readonly TextBox _txtDireccion = UiFactory.CreateTextBox(multiline: true);
    private readonly TextBox _txtAlergias = UiFactory.CreateTextBox(multiline: true);
    private readonly TextBox _txtAntecedentes = UiFactory.CreateTextBox(multiline: true);
    private readonly TextBox _txtContactoEmergenciaNombre = UiFactory.CreateTextBox();
    private readonly TextBox _txtContactoEmergenciaTelefono = UiFactory.CreateTextBox();
    private readonly TextBox _txtSeguroMedico = UiFactory.CreateTextBox();
    private readonly TextBox _txtNumeroTarjetaSanitaria = UiFactory.CreateTextBox();
    private readonly TextBox _txtInformacionExtra = UiFactory.CreateTextBox(multiline: true);
    private readonly ComboBox _cmbSexo = UiFactory.CreateComboBox();
    private readonly ComboBox _cmbGrupoSanguineo = UiFactory.CreateComboBox();
    private readonly NumericUpDown _numAltura = CreateNumericInput(0, 260, 1);
    private readonly NumericUpDown _numPeso = CreateNumericInput(0, 350, 1);
    private readonly DateTimePicker _dtpNacimiento = UiFactory.CreateDatePicker();

    private MediTrack.Core.Models.Patient? _loadedPatient;
    private string? _pendingPhotoPath;

    public PerfilPacienteForm(ApplicationServices services, AppSession session) : base("Mi perfil")
    {
        _services = services;
        _session = session;

        ConfigureSelectors();

        var page = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = AppTheme.Background,
            AutoScroll = true
        };

        var summaryCard = BuildSummaryCard();
        var formCard = BuildFormCard();

        var stacked = false;
        ApplyResponsiveLayout(page, summaryCard, formCard, stacked);
        page.Resize += (_, _) =>
        {
            var shouldStack = page.ClientSize.Width < 920;
            if (shouldStack == stacked)
            {
                return;
            }

            stacked = shouldStack;
            ApplyResponsiveLayout(page, summaryCard, formCard, stacked);
        };

        ContentPanel.Controls.Add(page);
        Load += async (_, _) => await LoadProfileAsync();
    }

    private static NumericUpDown CreateNumericInput(decimal min, decimal max, int decimalPlaces)
    {
        var input = new NumericUpDown
        {
            Minimum = min,
            Maximum = max,
            DecimalPlaces = decimalPlaces,
            Increment = 0.5m,
            Height = 36,
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
            ThousandsSeparator = false
        };
        AppTheme.ApplyTextInputStyle(input);
        return input;
    }

    private void ConfigureSelectors()
    {
        _cmbSexo.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbSexo.Items.AddRange(new object[] { "Hombre", "Mujer" });
        _cmbGrupoSanguineo.Items.AddRange(new object[] { "No especificado", "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" });
        _cmbGrupoSanguineo.SelectedIndex = 0;
    }

    private static void ApplyResponsiveLayout(TableLayoutPanel page, Control summaryCard, Control formCard, bool stacked)
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
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 390));
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 1120));
            page.Controls.Add(summaryCard, 0, 0);
            page.Controls.Add(formCard, 0, 1);
        }
        else
        {
            page.ColumnCount = 2;
            page.RowCount = 1;
            page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
            page.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            page.Controls.Add(summaryCard, 0, 0);
            page.Controls.Add(formCard, 1, 0);
        }

        page.ResumeLayout();
    }

    private Control BuildSummaryCard()
    {
        var summaryCard = AppTheme.CreateMutedPanel();
        summaryCard.Padding = new Padding(24);
        var summaryLayout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true
        };

        var btnPhoto = UiFactory.CreateButton("Seleccionar imagen", false);
        btnPhoto.Width = 180;
        btnPhoto.Click += (_, _) => SelectPhoto();

        summaryLayout.Controls.Add(UiFactory.CreateSectionTitle("Resumen del paciente"));
        summaryLayout.Controls.Add(_picFoto);
        summaryLayout.Controls.Add(btnPhoto);
        summaryLayout.Controls.Add(_lblNombre);
        summaryLayout.Controls.Add(_lblEmail);
        summaryLayout.Controls.Add(_lblEdad);
        summaryLayout.Controls.Add(UiFactory.CreateParagraphLabel("Mantener este perfil actualizado ayuda a que el seguimiento clÃ­nico, los informes y las notas mÃ©dicas sean mÃ¡s Ãºtiles y precisos.", 96));
        summaryCard.Controls.Add(summaryLayout);
        return summaryCard;
    }

    private Control BuildFormCard()
    {
        var card = AppTheme.CreateCardPanel();
        card.Padding = new Padding(28);
        var form = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            AutoScroll = true,
            BackColor = AppTheme.Surface
        };
        form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        _txtDireccion.Height = 96;
        _txtAlergias.Height = 110;
        _txtAntecedentes.Height = 130;
        _txtInformacionExtra.Height = 130;

        var row = 0;
        AddPairedFields(form, ref row, "DNI/NIE", _txtDniNie, "Fecha de nacimiento", _dtpNacimiento);
        AddPairedFields(form, ref row, "Sexo", _cmbSexo, "Grupo sanguÃ­neo", _cmbGrupoSanguineo);
        AddPairedFields(form, ref row, "Altura (cm)", _numAltura, "Peso (kg)", _numPeso);
        AddPairedFields(form, ref row, "TelÃ©fono", _txtTelefono, "DirecciÃ³n", _txtDireccion);
        AddPairedFields(form, ref row, "Contacto de emergencia", _txtContactoEmergenciaNombre, "TelÃ©fono de emergencia", _txtContactoEmergenciaTelefono);
        AddPairedFields(form, ref row, "Seguro mÃ©dico", _txtSeguroMedico, "NÃºmero de tarjeta sanitaria", _txtNumeroTarjetaSanitaria);
        AddWideField(form, ref row, "Alergias", _txtAlergias);
        AddWideField(form, ref row, "Antecedentes mÃ©dicos", _txtAntecedentes);
        AddWideField(form, ref row, "InformaciÃ³n extra del paciente", _txtInformacionExtra);

        var help = UiFactory.CreateInfoPanel("Puedes indicar cualquier situaciÃ³n relevante: si el paciente vive solo, si cuenta con apoyo familiar, limitaciones de movilidad u otra informaciÃ³n que ayude al seguimiento.", 86);
        form.Controls.Add(help, 0, row++);
        form.SetColumnSpan(help, 2);

        var actionPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            WrapContents = true,
            Margin = new Padding(0, 10, 0, 0)
        };
        var btnSave = UiFactory.CreateButton("Guardar cambios");
        btnSave.Click += async (_, _) => await SaveAsync();
        actionPanel.Controls.Add(btnSave);
        form.Controls.Add(actionPanel, 0, row);
        form.SetColumnSpan(actionPanel, 2);

        card.Controls.Add(form);
        return card;
    }

    private static void AddPairedFields(TableLayoutPanel form, ref int row, string leftLabel, Control leftControl, string rightLabel, Control rightControl)
    {
        form.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        form.Controls.Add(UiFactory.CreateLabel(leftLabel), 0, row);
        form.Controls.Add(UiFactory.CreateLabel(rightLabel), 1, row++);

        form.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        leftControl.Dock = DockStyle.Top;
        rightControl.Dock = DockStyle.Top;
        leftControl.Margin = new Padding(0, 0, 8, 14);
        rightControl.Margin = new Padding(8, 0, 0, 14);
        form.Controls.Add(leftControl, 0, row);
        form.Controls.Add(rightControl, 1, row++);
    }

    private static void AddWideField(TableLayoutPanel form, ref int row, string label, Control control)
    {
        form.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var labelControl = UiFactory.CreateLabel(label);
        form.Controls.Add(labelControl, 0, row++);
        form.SetColumnSpan(labelControl, 2);

        form.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        control.Dock = DockStyle.Top;
        control.Margin = new Padding(0, 0, 0, 14);
        form.Controls.Add(control, 0, row++);
        form.SetColumnSpan(control, 2);
    }

    private async Task LoadProfileAsync()
    {
        var patient = await _services.PatientService.GetByUserIdAsync(_session.CurrentUser!.Id);
        if (patient == null)
        {
            return;
        }

        _loadedPatient = patient;
        _lblNombre.Text = $"Paciente: {_session.CurrentUser.NombreCompleto}";
        _lblEmail.Text = $"Correo: {_session.CurrentUser.Email}";
        _lblEdad.Text = $"Edad: {CalculateAge(patient.FechaNacimiento)} años";
        _txtDniNie.Text = patient.DniNie;
        _dtpNacimiento.Value = patient.FechaNacimiento;
        SetSexValue(patient.Sexo);
        SetComboValue(_cmbGrupoSanguineo, patient.GrupoSanguineo);
        _numAltura.Value = Clamp(patient.AlturaCm, _numAltura.Minimum, _numAltura.Maximum);
        _numPeso.Value = Clamp(patient.PesoKg, _numPeso.Minimum, _numPeso.Maximum);
        _txtTelefono.Text = patient.Telefono;
        _txtDireccion.Text = patient.Direccion;
        _txtAlergias.Text = patient.Alergias;
        _txtAntecedentes.Text = patient.AntecedentesMedicos;
        _txtContactoEmergenciaNombre.Text = patient.ContactoEmergenciaNombre;
        _txtContactoEmergenciaTelefono.Text = patient.ContactoEmergenciaTelefono;
        _txtSeguroMedico.Text = patient.SeguroMedico;
        _txtNumeroTarjetaSanitaria.Text = patient.NumeroTarjetaSanitaria;
        _txtInformacionExtra.Text = patient.ObservacionesGenerales;
        LoadPhoto(patient.FotoRuta);
    }

    private void SelectPhoto()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Seleccionar imagen del paciente",
            Filter = "ImÃ¡genes|*.jpg;*.jpeg;*.png;*.bmp;*.gif|Todos los archivos|*.*"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        _pendingPhotoPath = dialog.FileName;
        LoadPhotoFromAbsolutePath(dialog.FileName);
    }

    private async Task SaveAsync()
    {
        if (_loadedPatient == null)
        {
            return;
        }

        if (_cmbSexo.SelectedItem is not string sexo || (sexo != "Hombre" && sexo != "Mujer"))
        {
            MessageBox.Show("Selecciona el sexo del paciente: Hombre o Mujer.", "MediTrack", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var fotoRuta = CopyPhotoIfNeeded(_loadedPatient.IdUsuario, _loadedPatient.FotoRuta);
        var patient = new MediTrack.Core.Models.Patient
        {
            IdUsuario = _loadedPatient.IdUsuario,
            FechaNacimiento = _dtpNacimiento.Value.Date,
            Telefono = _txtTelefono.Text,
            Direccion = _txtDireccion.Text,
            ObservacionesGenerales = _txtInformacionExtra.Text,
            DniNie = _txtDniNie.Text,
            Sexo = sexo,
            GrupoSanguineo = _cmbGrupoSanguineo.Text,
            AlturaCm = _numAltura.Value,
            PesoKg = _numPeso.Value,
            Alergias = _txtAlergias.Text,
            AntecedentesMedicos = _txtAntecedentes.Text,
            ContactoEmergenciaNombre = _txtContactoEmergenciaNombre.Text,
            ContactoEmergenciaTelefono = _txtContactoEmergenciaTelefono.Text,
            SeguroMedico = _txtSeguroMedico.Text,
            NumeroTarjetaSanitaria = _txtNumeroTarjetaSanitaria.Text,
            FotoRuta = fotoRuta
        };

        var result = await _services.PatientService.UpdateAsync(patient);
        if (result.Success)
        {
            _loadedPatient = patient;
            _pendingPhotoPath = null;
            await LoadProfileAsync();
        }

        MessageBox.Show(result.Message, "MediTrack", MessageBoxButtons.OK,
            result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
    }

    private string CopyPhotoIfNeeded(Guid patientId, string currentRelativePath)
    {
        if (string.IsNullOrWhiteSpace(_pendingPhotoPath) || !File.Exists(_pendingPhotoPath))
        {
            return currentRelativePath;
        }

        var photosFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "FotosPacientes");
        Directory.CreateDirectory(photosFolder);

        var extension = Path.GetExtension(_pendingPhotoPath);
        var fileName = $"{patientId:N}_{Guid.NewGuid():N}{extension}";
        var destination = Path.Combine(photosFolder, fileName);
        File.Copy(_pendingPhotoPath, destination, overwrite: true);
        return Path.Combine("FotosPacientes", fileName);
    }

    private void LoadPhoto(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            ClearPhoto();
            return;
        }

        var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", relativePath);
        LoadPhotoFromAbsolutePath(fullPath);
    }

    private void LoadPhotoFromAbsolutePath(string path)
    {
        if (!File.Exists(path))
        {
            ClearPhoto();
            return;
        }

        var previous = _picFoto.Image;
        using var image = Image.FromFile(path);
        _picFoto.Image = new Bitmap(image);
        previous?.Dispose();
    }

    private void ClearPhoto()
    {
        var previous = _picFoto.Image;
        _picFoto.Image = null;
        previous?.Dispose();
    }

    private static void SetComboValue(ComboBox combo, string value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? "No especificado" : value.Trim();
        if (!combo.Items.Contains(normalized))
        {
            combo.Items.Add(normalized);
        }

        combo.SelectedItem = normalized;
    }

    private void SetSexValue(string value)
    {
        var normalized = value.Trim();
        _cmbSexo.SelectedItem = normalized is "Hombre" or "Mujer" ? normalized : null;
    }

    private static int CalculateAge(DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age))
        {
            age--;
        }

        return Math.Max(0, age);
    }

    private static decimal Clamp(decimal value, decimal min, decimal max)
    {
        if (value < min)
        {
            return min;
        }

        return value > max ? max : value;
    }
}

