using MediTrack.Core.Interfaces.Repositories;
using MediTrack.Core.Services;
using MediTrack.Data.Config;
using MediTrack.Data.Repositories;
using MediTrack.Data.Seed;

namespace MediTrack.WinForms.Config;

public static class AppBootstrapper
{
    public static async Task<ApplicationServices> BuildAsync()
    {
        var dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
        var storagePaths = new StoragePaths(dataPath);

        IUserRepository userRepository = new UserRepository(storagePaths);
        IPatientRepository patientRepository = new PatientRepository(storagePaths);
        IDoctorRepository doctorRepository = new DoctorRepository(storagePaths);
        IAdministratorRepository administratorRepository = new AdministratorRepository(storagePaths);
        IChronicDiseaseRepository chronicDiseaseRepository = new ChronicDiseaseRepository(storagePaths);
        IPatientDiseaseRepository patientDiseaseRepository = new PatientDiseaseRepository(storagePaths);
        IMedicationRepository medicationRepository = new MedicationRepository(storagePaths);
        IMeasurementRepository measurementRepository = new MeasurementRepository(storagePaths);
        IMedicalNoteRepository medicalNoteRepository = new MedicalNoteRepository(storagePaths);
        IReportRepository reportRepository = new ReportRepository(storagePaths);
        IDoctorPatientAssignmentRepository assignmentRepository = new DoctorPatientAssignmentRepository(storagePaths);

        var authService = new AuthService(userRepository, patientRepository);
        var userService = new UserService(userRepository, patientRepository, doctorRepository, administratorRepository);
        var patientService = new PatientService(patientRepository, userRepository, assignmentRepository, patientDiseaseRepository, chronicDiseaseRepository);
        var diseaseService = new DiseaseService(chronicDiseaseRepository, patientDiseaseRepository);
        var medicationService = new MedicationService(medicationRepository);
        var measurementService = new MeasurementService(measurementRepository);
        var noteService = new MedicalNoteService(medicalNoteRepository);
        var assignmentService = new DoctorAssignmentService(assignmentRepository, userRepository);
        var reportService = new ReportService(userRepository, patientRepository, diseaseService, medicationService, measurementService, noteService, assignmentService, reportRepository);

        var seeder = new DataSeeder(
            userRepository,
            patientRepository,
            doctorRepository,
            administratorRepository,
            chronicDiseaseRepository,
            patientDiseaseRepository,
            medicationRepository,
            measurementRepository,
            medicalNoteRepository,
            assignmentRepository);

        await seeder.SeedAsync();

        return new ApplicationServices
        {
            StoragePaths = storagePaths,
            AuthService = authService,
            UserService = userService,
            PatientService = patientService,
            DiseaseService = diseaseService,
            MedicationService = medicationService,
            MeasurementService = measurementService,
            MedicalNoteService = noteService,
            DoctorAssignmentService = assignmentService,
            ReportService = reportService
        };
    }
}
