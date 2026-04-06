namespace MediTrack.Core.DTOs;

public class RegisterRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public int Edad { get; set; }
    public string Email { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmarPassword { get; set; } = string.Empty;
}
