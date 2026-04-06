namespace MediTrack.Core.Models;

public class PatientDisease
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PacienteId { get; set; }
    public Guid EnfermedadId { get; set; }
    public DateTime FechaDiagnostico { get; set; } = DateTime.Today;
    public string Observaciones { get; set; } = string.Empty;
}
