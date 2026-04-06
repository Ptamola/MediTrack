using MediTrack.Core.Interfaces.Services;
using MediTrack.Data.Config;

namespace MediTrack.WinForms.Config;

public class ApplicationServices
{
    public required StoragePaths StoragePaths { get; init; }
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
