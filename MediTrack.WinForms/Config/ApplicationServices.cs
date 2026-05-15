using MediTrack.Core.Interfaces.Services;

namespace MediTrack.WinForms.Config;

/// <summary>
/// Agrupa los servicios de negocio que consumen los formularios.
/// Actua como un contenedor simple de dependencias para mantener la UI desacoplada de los repositorios.
/// </summary>
public class ApplicationServices
{
    public required IAuthService AuthService { get; init; }
    public required IUserService UserService { get; init; }
    public required IPatientService PatientService { get; init; }
    public required IDiseaseService DiseaseService { get; init; }
    public required IMedicationService MedicationService { get; init; }
    public required IMeasurementService MeasurementService { get; init; }
    public required IMedicalNoteService MedicalNoteService { get; init; }
    public required IDoctorAssignmentService DoctorAssignmentService { get; init; }
    public required IReportService ReportService { get; init; }
}
