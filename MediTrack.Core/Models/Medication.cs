namespace MediTrack.Core.Models;

public class Medication
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PacienteId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Dosis { get; set; } = string.Empty;
    public string Frecuencia { get; set; } = string.Empty;
    public string Horario { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; } = DateTime.Today;
    public DateTime? FechaFin { get; set; }
    public string Observaciones { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
