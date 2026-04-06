using MediTrack.Core.Enums;
using MediTrack.Core.Models;
using MediTrack.WinForms.Config;
using MediTrack.WinForms.Helpers;

namespace MediTrack.WinForms.Forms.Shared;

public class NotasMedicasForm : BaseModuleForm
{
    private readonly ApplicationServices _services;
    private readonly AppSession _session;
    private readonly ComboBox _cmbPacientes = UiFactory.CreateComboBox();
    private readonly TextBox _txtTitulo = UiFactory.CreateTextBox();
    private readonly TextBox _txtContenido = UiFactory.CreateTextBox(multiline: true);
    private readonly CheckBox _chkVisible = new() { Text = "Visible para el paciente", Checked = true, AutoSize = true };
    private readonly DataGridView _grid = UiFactory.CreateGrid();
    private readonly Label _lblHint = UiFactory.CreateMutedLabel("Consulta el historial de notas y registra nuevas observaciones si eres doctor.");

    public NotasMedicasForm(ApplicationServices services, AppSession session) : base("Notas medicas")
    {
        _services = services;
        _session = session;

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            SplitterDistance = 360,
            BackColor = AppTheme.Background
        };

        var left = AppTheme.CreateCardPanel();
        left.Padding = new Padding(24);
        var form = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            AutoScroll = true
        };
        form.Controls.Add(UiFactory.CreateSectionTitle("Registro clinico"), 0, 0);
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
                await LoadNotesAsync();
            };
        }

        if (_session.CurrentUser?.Rol == UserRole.Doctor)
        {
            form.Controls.Add(UiFactory.CreateLabel("Titulo"), 0, row++);
            form.Controls.Add(_txtTitulo, 0, row++);
            form.Controls.Add(UiFactory.CreateLabel("Contenido"), 0, row++);
            form.Controls.Add(_txtContenido, 0, row++);
            form.Controls.Add(_chkVisible, 0, row++);

            var btnAdd = UiFactory.CreateButton("Guardar nota");
            btnAdd.Click += async (_, _) => await SaveAsync();
            form.Controls.Add(btnAdd, 0, row++);
        }
        else if (_session.CurrentUser?.Rol == UserRole.Paciente)
        {
            _lblHint.Text = "Estas viendo las notas visibles registradas por tu doctor.";
        }
        else
        {
            _lblHint.Text = "Consulta el historial de notas del paciente seleccionado.";
        }

        left.Controls.Add(form);

        var right = AppTheme.CreateCardPanel();
        right.Padding = new Padding(20);
        var rightLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
        rightLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        rightLayout.Controls.Add(UiFactory.CreateSectionTitle("Historial de notas"), 0, 0);
        rightLayout.Controls.Add(_grid, 0, 1);
        right.Controls.Add(rightLayout);

        split.Panel1.Controls.Add(left);
        split.Panel2.Controls.Add(right);
        ContentPanel.Controls.Add(split);

        Load += async (_, _) => await InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        if (_session.CurrentUser?.Rol != UserRole.Paciente)
        {
            var patients = await PatientAccessHelper.GetAccessiblePatientsAsync(_services, _session);
            _cmbPacientes.DataSource = patients;
            _cmbPacientes.DisplayMember = "NombreCompleto";
            _cmbPacientes.ValueMember = "Id";
            await PatientAccessHelper.EnsureSelectedPatientAsync(_services, _session);
            PatientAccessHelper.ApplySelectedPatient(_cmbPacientes, _session);
            if (_session.CurrentUser?.Rol == UserRole.Doctor)
            {
                _lblHint.Text = "Selecciona un paciente para registrar notas clinicas y compartirlas con el paciente si procede.";
            }
        }

        await LoadNotesAsync();
    }

    private Guid CurrentPatientId => _session.CurrentUser?.Rol == UserRole.Paciente
        ? _session.CurrentUser!.Id
        : _cmbPacientes.SelectedValue is Guid patientId ? patientId : Guid.Empty;

    private async Task SaveAsync()
    {
        var note = new MedicalNote
        {
            PacienteId = CurrentPatientId,
            DoctorId = _session.CurrentUser!.Id,
            Titulo = _txtTitulo.Text,
            Contenido = _txtContenido.Text,
            VisibleParaPaciente = _chkVisible.Checked
        };

        var result = await _services.MedicalNoteService.SaveAsync(note);
        MessageBox.Show(result.Message, "MediTrack");
        if (result.Success)
        {
            _txtTitulo.Clear();
            _txtContenido.Clear();
            _chkVisible.Checked = true;
        }
        await LoadNotesAsync();
    }

    private async Task LoadNotesAsync()
    {
        if (CurrentPatientId == Guid.Empty)
        {
            _grid.DataSource = null;
            return;
        }

        var notes = await _services.MedicalNoteService.GetByPatientAsync(CurrentPatientId, _session.CurrentUser?.Rol == UserRole.Paciente);
        _grid.DataSource = notes.Select(n => new
        {
            Fecha = n.FechaHora.ToString("dd/MM/yyyy HH:mm"),
            n.Titulo,
            n.Contenido,
            Visible = n.VisibleParaPaciente ? "Si" : "No"
        }).ToList();
    }
}
