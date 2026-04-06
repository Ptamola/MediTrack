using MediTrack.Core.Enums;
using MediTrack.Core.Models;
using MediTrack.WinForms.Config;
using MediTrack.WinForms.Forms.Shared;
using MediTrack.WinForms.Helpers;

namespace MediTrack.WinForms.Forms.Admin;

public class GestionUsuariosForm : BaseModuleForm
{
    private readonly ApplicationServices _services;
    private readonly TextBox _txtNombre = UiFactory.CreateTextBox();
    private readonly TextBox _txtApellidos = UiFactory.CreateTextBox();
    private readonly TextBox _txtEmail = UiFactory.CreateTextBox();
    private readonly TextBox _txtUsuario = UiFactory.CreateTextBox();
    private readonly TextBox _txtPassword = UiFactory.CreateTextBox(password: true);
    private readonly ComboBox _cmbRol = UiFactory.CreateComboBox();
    private readonly ComboBox _cmbFiltroRol = UiFactory.CreateComboBox();
    private readonly DataGridView _grid = UiFactory.CreateGrid();
    private readonly Label _lblHint = UiFactory.CreateMutedLabel("Crea usuarios nuevos, edita sus datos básicos y cambia su estado cuando sea necesario.");
    private List<User> _items = [];
    private Guid? _selectedUserId;
    private readonly Panel _editorCard;
    private readonly Panel _gridCard;
    private readonly TableLayoutPanel _stack;
    private readonly Panel _scrollHost;

    public GestionUsuariosForm(ApplicationServices services, AppSession session) : base("Gestión de usuarios")
    {
        _services = services;

        _scrollHost = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = AppTheme.Background
        };

        _stack = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            RowCount = 2,
            Dock = DockStyle.Top,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };
        _stack.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        _stack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _stack.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _editorCard = (Panel)BuildEditorCard();
        _editorCard.Margin = new Padding(0, 0, 0, 18);
        _gridCard = (Panel)BuildGridCard();
        _gridCard.Margin = new Padding(0, 0, 0, 0);

        _stack.Controls.Add(_editorCard, 0, 0);
        _stack.Controls.Add(_gridCard, 0, 1);
        _scrollHost.Controls.Add(_stack);
        ContentPanel.Controls.Add(_scrollHost);

        ContentPanel.Resize += (_, _) => UpdateLayoutWidths();
        Load += async (_, _) =>
        {
            UpdateLayoutWidths();
            await InitializeAsync();
        };
    }

    private Control BuildEditorCard()
    {
        var card = AppTheme.CreateCardPanel();
        card.Padding = new Padding(28, 24, 28, 24);

        var host = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2
        };
        host.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        host.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 5,
            Margin = new Padding(0)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var title = UiFactory.CreateSectionTitle("Ficha de usuario");
        layout.Controls.Add(title, 0, 0);
        layout.SetColumnSpan(title, 2);
        layout.Controls.Add(_lblHint, 0, 1);
        layout.SetColumnSpan(_lblHint, 2);

        layout.Controls.Add(BuildFieldPanel("Nombre", _txtNombre), 0, 2);
        layout.Controls.Add(BuildFieldPanel("Apellidos", _txtApellidos), 1, 2);
        layout.Controls.Add(BuildFieldPanel("Correo electrónico", _txtEmail), 0, 3);
        layout.Controls.Add(BuildFieldPanel("Nombre de usuario", _txtUsuario), 1, 3);
        layout.Controls.Add(BuildFieldPanel("Contraseña inicial", _txtPassword), 0, 4);
        layout.Controls.Add(BuildFieldPanel("Rol del sistema", _cmbRol), 1, 4);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoSize = true,
            Padding = new Padding(0),
            Margin = new Padding(0, 8, 0, 0)
        };

        var btnCreate = UiFactory.CreateButton("Crear usuario");
        btnCreate.Width = 190;
        btnCreate.Click += async (_, _) => await CreateAsync();
        var btnUpdate = UiFactory.CreateButton("Guardar edición", false);
        btnUpdate.Width = 190;
        btnUpdate.Click += async (_, _) => await UpdateAsync();
        var btnToggle = UiFactory.CreateDangerButton("Cambiar estado");
        btnToggle.Width = 190;
        btnToggle.Click += async (_, _) => await ToggleAsync();
        actions.Controls.Add(btnCreate);
        actions.Controls.Add(btnUpdate);
        actions.Controls.Add(btnToggle);

        _cmbRol.DataSource = Enum.GetValues(typeof(UserRole));
        host.Controls.Add(layout, 0, 0);
        host.Controls.Add(actions, 0, 1);
        card.Controls.Add(host);
        return card;
    }

    private Control BuildGridCard()
    {
        var card = AppTheme.CreateCardPanel();
        card.Padding = new Padding(22);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        layout.Controls.Add(UiFactory.CreateSectionTitle("Usuarios del sistema"), 0, 0);

        var filterRow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Height = 54,
            Margin = new Padding(0, 4, 0, 12)
        };
        var lblFiltro = UiFactory.CreateLabel("Filtrar por rol");
        lblFiltro.Margin = new Padding(0, 10, 8, 0);
        _cmbFiltroRol.Width = 220;
        _cmbFiltroRol.SelectedIndexChanged += (_, _) => ApplyFilter();
        filterRow.Controls.Add(lblFiltro);
        filterRow.Controls.Add(_cmbFiltroRol);
        layout.Controls.Add(filterRow, 0, 1);

        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.ScrollBars = ScrollBars.Vertical;
        _grid.CellClick += (_, e) => LoadSelected(e.RowIndex);
        layout.Controls.Add(_grid, 0, 2);

        card.Controls.Add(layout);
        return card;
    }

    private static Panel BuildFieldPanel(string labelText, Control input)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0),
            Margin = new Padding(0, 10, 16, 0),
            Height = 88
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2
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
        _cmbFiltroRol.Items.Clear();
        _cmbFiltroRol.Items.Add("Todos");
        _cmbFiltroRol.Items.AddRange(Enum.GetNames(typeof(UserRole)));
        _cmbFiltroRol.SelectedIndex = 0;

        await LoadUsersAsync();
    }

    private async Task LoadUsersAsync()
    {
        _items = await _services.UserService.GetAllAsync();
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        IEnumerable<User> source = _items;
        if (_cmbFiltroRol.SelectedItem is string selectedRole && selectedRole != "Todos" &&
            Enum.TryParse<UserRole>(selectedRole, out var role))
        {
            source = source.Where(u => u.Rol == role);
        }

        _grid.DataSource = source
            .OrderBy(u => u.Apellidos)
            .ThenBy(u => u.Nombre)
            .Select(u => new
            {
                u.Id,
                Nombre = u.NombreCompleto,
                Email = u.Email,
                Usuario = u.NombreUsuario,
                Rol = u.Rol.ToString(),
                Estado = u.Activo ? "Activo" : "Inactivo"
            })
            .ToList();

        if (_grid.Columns.Contains("Id"))
        {
            _grid.Columns["Id"].Visible = false;
        }

        if (_grid.Columns.Contains("Nombre"))
        {
            _grid.Columns["Nombre"].FillWeight = 24;
            _grid.Columns["Email"].FillWeight = 28;
            _grid.Columns["Usuario"].FillWeight = 18;
            _grid.Columns["Rol"].FillWeight = 14;
            _grid.Columns["Estado"].FillWeight = 16;
        }
    }

    private async Task CreateAsync()
    {
        var user = new User
        {
            Nombre = _txtNombre.Text,
            Apellidos = _txtApellidos.Text,
            Email = _txtEmail.Text,
            NombreUsuario = _txtUsuario.Text,
            Rol = (UserRole)_cmbRol.SelectedItem!,
            Activo = true
        };

        var result = await _services.UserService.CreateAsync(user, _txtPassword.Text);
        MessageBox.Show(result.Message, "MediTrack");
        if (result.Success)
        {
            ClearForm();
        }

        await LoadUsersAsync();
    }

    private async Task UpdateAsync()
    {
        if (!_selectedUserId.HasValue)
        {
            MessageBox.Show("Selecciona primero un usuario del listado.", "MediTrack");
            return;
        }

        var existing = _items.FirstOrDefault(x => x.Id == _selectedUserId.Value);
        if (existing == null)
        {
            MessageBox.Show("No se encontró el usuario seleccionado.", "MediTrack");
            return;
        }

        existing.Nombre = _txtNombre.Text;
        existing.Apellidos = _txtApellidos.Text;
        existing.Email = _txtEmail.Text;
        existing.NombreUsuario = _txtUsuario.Text;
        existing.Rol = (UserRole)_cmbRol.SelectedItem!;

        var result = await _services.UserService.UpdateAsync(existing);
        MessageBox.Show(result.Message, "MediTrack");
        if (result.Success)
        {
            ClearForm();
        }

        await LoadUsersAsync();
    }

    private async Task ToggleAsync()
    {
        if (_grid.CurrentRow?.Cells["Id"].Value is Guid id)
        {
            var user = _items.First(x => x.Id == id);
            var result = await _services.UserService.ToggleActiveAsync(id, !user.Activo);
            MessageBox.Show(result.Message, "MediTrack");
            await LoadUsersAsync();
        }
    }

    private void LoadSelected(int rowIndex)
    {
        if (rowIndex < 0 || _grid.Rows[rowIndex].Cells["Id"].Value is not Guid id)
        {
            return;
        }

        var user = _items.FirstOrDefault(x => x.Id == id);
        if (user == null)
        {
            return;
        }

        _selectedUserId = user.Id;
        _txtNombre.Text = user.Nombre;
        _txtApellidos.Text = user.Apellidos;
        _txtEmail.Text = user.Email;
        _txtUsuario.Text = user.NombreUsuario;
        _cmbRol.SelectedItem = user.Rol;
        _txtPassword.Clear();
    }

    private void ClearForm()
    {
        _selectedUserId = null;
        _txtNombre.Clear();
        _txtApellidos.Clear();
        _txtEmail.Clear();
        _txtUsuario.Clear();
        _txtPassword.Clear();
        _cmbRol.SelectedItem = UserRole.Paciente;
    }

    private void UpdateLayoutWidths()
    {
        var width = Math.Max(900, _scrollHost.ClientSize.Width - 6);
        _stack.Width = width;
        _editorCard.Width = width;
        _gridCard.Width = width;
        _editorCard.MinimumSize = new Size(width, 470);
        _gridCard.MinimumSize = new Size(width, 420);
    }
}
