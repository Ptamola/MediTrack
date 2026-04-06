namespace MediTrack.Core.Models;

public class Doctor
{
    public Guid IdUsuario { get; set; }
    public string NumeroColegiado { get; set; } = string.Empty;
    public string Especialidad { get; set; } = string.Empty;
}
