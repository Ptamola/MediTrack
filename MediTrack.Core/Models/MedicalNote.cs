namespace MediTrack.Core.Models;

public class MedicalNote
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PacienteId { get; set; }
    public Guid DoctorId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Contenido { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; } = DateTime.Now;
    public bool VisibleParaPaciente { get; set; } = true;
}
