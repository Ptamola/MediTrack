using MediTrack.Core.Enums;
using MediTrack.Core.Models;
using MediTrack.WinForms.Config;
using MediTrack.WinForms.Helpers;

namespace MediTrack.WinForms.Forms.Shared;

/// <summary>
/// Pantalla compartida de notas medicas. El doctor crea notas y el paciente solo ve
/// las marcadas como visibles.
/// </summary>
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

    public NotasMedicasForm(ApplicationServices services, AppSession session) : base("Notas médicas")
    {
        _services = services;
        _session = session;

        ConfigureNotesGrid();
        _txtContenido.Height = 150;

        var historyCard = CreateHistoryCard();
        if (_session.CurrentUser?.Rol == UserRole.Paciente)
        {
            _lblHint.Text = "Estás viendo las notas visibles registradas por tu doctor.";
            ContentPanel.Controls.Add(CreatePatientLayout(historyCard));
        }
        else
        {
            ContentPanel.Controls.Add(CreateDoctorLayout(CreateEditorCard(), historyCard));
        }

        Load += async (_, _) => await InitializeAsync();
    }

    private Control CreateEditorCard()
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
        _lblHint.AutoSize = false;
        _lblHint.Dock = DockStyle.Fill;
        _lblHint.Height = 56;

        form.Controls.Add(UiFactory.CreateSectionTitle("Registro clínico"), 0, 0);
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
            form.Controls.Add(UiFactory.CreateLabel("Título"), 0, row++);
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
            _lblHint.Text = "Estás viendo las notas visibles registradas por tu doctor.";
        }
        else
        {
            _lblHint.Text = "Consulta el historial de notas del paciente seleccionado.";
        }

        left.Controls.Add(form);
        return left;
    }

    private Control CreateHistoryCard()
    {
        var right = AppTheme.CreateCardPanel();
        right.Padding = new Padding(20);
        var rightLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1,
            BackColor = AppTheme.Surface
        };
        rightLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        rightLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        rightLayout.Controls.Add(UiFactory.CreateSectionTitle("Historial de notas"), 0, 0);
        rightLayout.Controls.Add(_grid, 0, 1);
        right.Controls.Add(rightLayout);
        return right;
    }

    private static Control CreatePatientLayout(Control historyCard)
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            AutoScroll = true,
            BackColor = AppTheme.Background
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var intro = AppTheme.CreateCardPanel();
        intro.Padding = new Padding(24);
        var introLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = AppTheme.Surface
        };
        introLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        introLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        introLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        introLayout.Controls.Add(UiFactory.CreateSectionTitle("Notas de tu seguimiento"), 0, 0);
        introLayout.Controls.Add(CreateWrappedMutedLabel("Aquí puedes consultar únicamente las notas que tu doctor ha marcado como visibles para ti."), 0, 1);
        intro.Controls.Add(introLayout);

        layout.Controls.Add(intro, 0, 0);
        layout.Controls.Add(historyCard, 0, 1);
        return layout;
    }

    private static Control CreateDoctorLayout(Control editorCard, Control historyCard)
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            AutoScroll = true,
            BackColor = AppTheme.Background
        };

        var isStacked = false;
        ApplyResponsiveLayout(layout, editorCard, historyCard, stacked: false);
        layout.Resize += (_, _) =>
        {
            var shouldStack = layout.ClientSize.Width < 1080;
            if (shouldStack == isStacked)
            {
                return;
            }

            isStacked = shouldStack;
            ApplyResponsiveLayout(layout, editorCard, historyCard, shouldStack);
        };

        return layout;
    }

    private static void ApplyResponsiveLayout(TableLayoutPanel layout, Control editorCard, Control historyCard, bool stacked)
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
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 440));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.Controls.Add(editorCard, 0, 0);
            layout.Controls.Add(historyCard, 0, 1);
        }
        else
        {
            layout.ColumnCount = 2;
            layout.RowCount = 1;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 66));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.Controls.Add(editorCard, 0, 0);
            layout.Controls.Add(historyCard, 1, 0);
        }

        layout.ResumeLayout();
    }

    private static Label CreateWrappedMutedLabel(string text) => new()
    {
        Text = text,
        Dock = DockStyle.Fill,
        AutoSize = false,
        Font = AppTheme.BodyFont,
        ForeColor = AppTheme.TextSecondary,
        TextAlign = ContentAlignment.TopLeft,
        Margin = new Padding(0)
    };

    private void ConfigureNotesGrid()
    {
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        _grid.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        _grid.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
        _grid.RowTemplate.MinimumHeight = 44;
        _grid.ScrollBars = ScrollBars.Both;
        _grid.AllowUserToResizeColumns = true;
        _grid.AllowUserToResizeRows = true;
    }

    /// <summary>
    /// Carga pacientes accesibles y el historial de notas segun el rol actual.
    /// </summary>
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
                _lblHint.Text = "Selecciona un paciente para registrar notas clínicas y compartirlas con el paciente si procede.";
            }
        }

        await LoadNotesAsync();
    }

    private Guid CurrentPatientId => _session.CurrentUser?.Rol == UserRole.Paciente
        ? _session.CurrentUser!.Id
        : _cmbPacientes.SelectedValue is Guid patientId ? patientId : Guid.Empty;

    /// <summary>
    /// Guarda una nota nueva escrita por el doctor.
    /// </summary>
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

    /// <summary>
    /// Recarga la tabla aplicando el filtro de visibilidad si el usuario es paciente.
    /// </summary>
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
            Visible = n.VisibleParaPaciente ? "Sí" : "No"
        }).ToList();
        ApplyNotesGridColumnLayout();
    }

    private void ApplyNotesGridColumnLayout()
    {
        if (_grid.Columns["Fecha"] is { } fechaColumn)
        {
            fechaColumn.HeaderText = "Fecha";
            fechaColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            fechaColumn.Width = 138;
            fechaColumn.MinimumWidth = 125;
            fechaColumn.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
        }

        if (_grid.Columns["Titulo"] is { } titleColumn)
        {
            titleColumn.HeaderText = "Título";
            titleColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            titleColumn.FillWeight = 24;
            titleColumn.MinimumWidth = 140;
        }

        if (_grid.Columns["Contenido"] is { } contentColumn)
        {
            contentColumn.HeaderText = "Contenido";
            contentColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            contentColumn.FillWeight = 62;
            contentColumn.MinimumWidth = 260;
            contentColumn.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        }

        if (_grid.Columns["Visible"] is { } visibleColumn)
        {
            visibleColumn.HeaderText = "Visible";
            visibleColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            visibleColumn.Width = 72;
            visibleColumn.MinimumWidth = 64;
            visibleColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }
    }
}
