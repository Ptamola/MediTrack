using MediTrack.Core.Interfaces.Repositories;
using MediTrack.Core.Services;
using MediTrack.Data.Config;
using MediTrack.Data.Repositories.MySql;
using MediTrack.Data.Seed;

namespace MediTrack.WinForms.Config;

public static class AppBootstrapper
{
    public static async Task<ApplicationServices> BuildAsync()
    {
        var databaseSettings = DatabaseSettings.FromEnvironment();
        var connectionFactory = new DatabaseConnectionFactory(databaseSettings);
        var databaseInitializer = new DatabaseInitializer(databaseSettings, connectionFactory);
        await databaseInitializer.InitializeAsync();

        var dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
        var storagePaths = new StoragePaths(dataPath);

        IUserRepository userRepository = new UserRepositoryMySql(connectionFactory);
        IPatientRepository patientRepository = new PatientRepositoryMySql(connectionFactory);
        IDoctorRepository doctorRepository = new DoctorRepositoryMySql(connectionFactory);
        IAdministratorRepository administratorRepository = new AdministratorRepositoryMySql(connectionFactory);
        IChronicDiseaseRepository chronicDiseaseRepository = new ChronicDiseaseRepositoryMySql(connectionFactory);
        IPatientDiseaseRepository patientDiseaseRepository = new PatientDiseaseRepositoryMySql(connectionFactory);
        IMedicationRepository medicationRepository = new MedicationRepositoryMySql(connectionFactory);
        IMeasurementRepository measurementRepository = new MeasurementRepositoryMySql(connectionFactory);
        IMedicalNoteRepository medicalNoteRepository = new MedicalNoteRepositoryMySql(connectionFactory);
        IReportRepository reportRepository = new ReportRepositoryMySql(connectionFactory);
        IDoctorPatientAssignmentRepository assignmentRepository = new DoctorPatientAssignmentRepositoryMySql(connectionFactory);

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
            assignmentRepository,
            reportRepository);

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
