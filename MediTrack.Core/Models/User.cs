using MediTrack.Core.Enums;

namespace MediTrack.Core.Models;

/// <summary>
/// Usuario autenticable de MediTrack. El rol determina la navegacion y permisos visibles.
/// </summary>
public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nombre { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Rol { get; set; } = UserRole.Paciente;
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public string NombreCompleto => $"{Nombre} {Apellidos}".Trim();
}
