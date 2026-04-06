namespace MediTrack.Core.Models;

public class Patient
{
    public Guid IdUsuario { get; set; }
    public DateTime FechaNacimiento { get; set; } = DateTime.Today.AddYears(-30);
    public string Telefono { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string ObservacionesGenerales { get; set; } = string.Empty;
}
