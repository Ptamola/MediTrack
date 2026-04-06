namespace MediTrack.Core.DTOs;

public class PatientSummaryDto
{
    public Guid PacienteId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Enfermedades { get; set; } = string.Empty;
}
