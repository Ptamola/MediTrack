using MediTrack.Core.Interfaces.Repositories;
using MediTrack.Core.Services;
using MediTrack.Data.Config;
using MediTrack.Data.Repositories.MySql;
using MediTrack.Data.Seed;

namespace MediTrack.WinForms.Config;

/// <summary>
/// Construye manualmente las dependencias principales de la aplicacion.
/// Aqui se conectan repositorios MySQL, servicios de negocio y semilla demo.
/// </summary>
public static class AppBootstrapper
{
    /// <summary>
    /// Inicializa MySQL, crea los repositorios concretos y devuelve el contenedor de servicios
    /// usado por los formularios WinForms.
    /// </summary>
    public static async Task<ApplicationServices> BuildAsync()
    {
        var databaseSettings = DatabaseSettings.FromEnvironment();
        var connectionFactory = new DatabaseConnectionFactory(databaseSettings);
        var databaseInitializer = new DatabaseInitializer(databaseSettings, connectionFactory);
        await databaseInitializer.InitializeAsync();

        // Los formularios trabajan contra interfaces; cambiar la persistencia no requiere tocar la UI.
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

        // La semilla solo inserta datos si la base esta vacia, evitando duplicados en ejecuciones posteriores.
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
