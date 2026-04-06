namespace MediTrack.Core.Models;

public class DoctorPatientAssignment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DoctorId { get; set; }
    public Guid PacienteId { get; set; }
    public DateTime FechaAsignacion { get; set; } = DateTime.Today;
    public bool Activa { get; set; } = true;
    public DateTime? FechaFinAsignacion { get; set; }
}
