using MediTrack.Core.Models;

namespace MediTrack.Core.DTOs;

public class GeneratedReportResult
{
    public Report Reporte { get; set; } = new();
    public string Titulo { get; set; } = string.Empty;
    public string ContenidoVistaPrevia { get; set; } = string.Empty;
    public User? PacienteUsuario { get; set; }
    public Patient? PacientePerfil { get; set; }
    public User? DoctorAsignado { get; set; }
    public List<User> DoctoresAnteriores { get; set; } = [];
    public List<ChronicDisease> CatalogoEnfermedades { get; set; } = [];
    public List<ChronicDisease> Enfermedades { get; set; } = [];
    public List<PatientDisease> EnfermedadesPaciente { get; set; } = [];
    public List<Medication> Medicamentos { get; set; } = [];
    public List<Measurement> Mediciones { get; set; } = [];
    public List<MedicalNote> Notas { get; set; } = [];
}
