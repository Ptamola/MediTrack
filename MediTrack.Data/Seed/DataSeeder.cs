using MediTrack.Core.Constants;
using MediTrack.Core.Enums;
using MediTrack.Core.Helpers;
using MediTrack.Core.Interfaces.Repositories;
using MediTrack.Core.Models;

namespace MediTrack.Data.Seed;

/// <summary>
/// Inserta datos de demostracion cuando la base de datos esta vacia.
/// Usa Guid fijos y repositorios MySQL para que la semilla sea repetible e idempotente.
/// </summary>
public class DataSeeder(
    IUserRepository userRepository,
    IPatientRepository patientRepository,
    IDoctorRepository doctorRepository,
    IAdministratorRepository administratorRepository,
    IChronicDiseaseRepository chronicDiseaseRepository,
    IPatientDiseaseRepository patientDiseaseRepository,
    IMedicationRepository medicationRepository,
    IMeasurementRepository measurementRepository,
    IMedicalNoteRepository medicalNoteRepository,
    IDoctorPatientAssignmentRepository assignmentRepository,
    IReportRepository reportRepository)
{
    private static readonly Guid AdminId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid DoctorId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid Patient1Id = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid Patient2Id = Guid.Parse("44444444-4444-4444-4444-444444444444");

    private static readonly Guid DiabetesId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");
    private static readonly Guid HypertensionId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000002");
    private static readonly Guid AsthmaId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000003");
    private static readonly Guid KidneyDiseaseId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000004");

    /// <summary>
    /// Carga usuarios, perfiles, enfermedades, medicacion, mediciones, notas e informes demo
    /// solo si todavia no existen usuarios en MySQL.
    /// </summary>
    public async Task SeedAsync()
    {
        var existingUsers = await userRepository.GetAllAsync();
        if (existingUsers.Count > 0)
        {
            return;
        }

        // A partir de aqui la base esta vacia; se crean datos completos para probar todos los roles.
        var today = DateTime.Today;
        var users = BuildUsers();
        await userRepository.SaveAllAsync(users);

        await administratorRepository.SaveAllAsync([new Administrator { IdUsuario = AdminId }]);
        await doctorRepository.SaveAllAsync(
        [
            new Doctor { IdUsuario = DoctorId, NumeroColegiado = "DEMO-1001", Especialidad = "Medicina interna" }
        ]);

        await patientRepository.SaveAllAsync(
        [
            new Patient
            {
                IdUsuario = Patient1Id,
                FechaNacimiento = new DateTime(2005, 4, 28),
                Telefono = "600100200",
                Direccion = "Calle Salud 12, Madrid",
                ObservacionesGenerales = "Paciente demo con buen apoyo familiar. Vive con sus padres y registra controles con regularidad.",
                DniNie = "12345678A",
                Sexo = "Mujer",
                GrupoSanguineo = "A+",
                AlturaCm = 168,
                PesoKg = 67,
                Alergias = "Sin alergias medicamentosas conocidas.",
                AntecedentesMedicos = "Diabetes tipo 2 diagnosticada en seguimiento. Asma infantil resuelta.",
                ContactoEmergenciaNombre = "Laura Martin",
                ContactoEmergenciaTelefono = "600100201",
                SeguroMedico = "Seguridad Social",
                NumeroTarjetaSanitaria = "TS-DEMO-001"
            },
            new Patient
            {
                IdUsuario = Patient2Id,
                FechaNacimiento = new DateTime(1968, 9, 15),
                Telefono = "600300400",
                Direccion = "Avenida Bienestar 24, Valencia",
                ObservacionesGenerales = "Paciente demo que vive solo. Conviene reforzar educación sanitaria y seguimiento de tensión arterial.",
                DniNie = "87654321B",
                Sexo = "Hombre",
                GrupoSanguineo = "O+",
                AlturaCm = 176,
                PesoKg = 84,
                Alergias = "Alergia referida a penicilina.",
                AntecedentesMedicos = "Hipertensión arterial y enfermedad renal crónica en control ambulatorio.",
                ContactoEmergenciaNombre = "Carmen Ruiz",
                ContactoEmergenciaTelefono = "600300401",
                SeguroMedico = "Seguridad Social",
                NumeroTarjetaSanitaria = "TS-DEMO-002"
            }
        ]);

        var diseases = BuildDiseases();
        await chronicDiseaseRepository.SaveAllAsync(diseases);

        await patientDiseaseRepository.SaveAllAsync(
        [
            new PatientDisease
            {
                Id = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001"),
                PacienteId = Patient1Id,
                EnfermedadId = DiabetesId,
                FechaDiagnostico = today.AddYears(-3),
                Activa = true,
                Observaciones = "Controlada con dieta, actividad física y metformina."
            },
            new PatientDisease
            {
                Id = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000002"),
                PacienteId = Patient1Id,
                EnfermedadId = AsthmaId,
                FechaDiagnostico = today.AddYears(-10),
                FechaFin = today.AddMonths(-8),
                Activa = false,
                Observaciones = "Asma infantil sin crisis recientes. Se marca como superada para conservar historial."
            },
            new PatientDisease
            {
                Id = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000003"),
                PacienteId = Patient2Id,
                EnfermedadId = HypertensionId,
                FechaDiagnostico = today.AddYears(-7),
                Activa = true,
                Observaciones = "Seguimiento mensual con control domiciliario de presión arterial."
            },
            new PatientDisease
            {
                Id = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000004"),
                PacienteId = Patient2Id,
                EnfermedadId = KidneyDiseaseId,
                FechaDiagnostico = today.AddYears(-2),
                Activa = true,
                Observaciones = "Control nefrológico semestral, sin progresión reciente."
            }
        ]);

        await assignmentRepository.SaveAllAsync(
        [
            new DoctorPatientAssignment
            {
                Id = Guid.Parse("cccccccc-0000-0000-0000-000000000001"),
                DoctorId = DoctorId,
                PacienteId = Patient1Id,
                FechaAsignacion = today.AddMonths(-9),
                Activa = true
            },
            new DoctorPatientAssignment
            {
                Id = Guid.Parse("cccccccc-0000-0000-0000-000000000002"),
                DoctorId = DoctorId,
                PacienteId = Patient2Id,
                FechaAsignacion = today.AddMonths(-6),
                Activa = true
            }
        ]);

        await medicationRepository.SaveAllAsync(
        [
            new Medication
            {
                Id = Guid.Parse("dddddddd-0000-0000-0000-000000000001"),
                PacienteId = Patient1Id,
                Nombre = "Metformina",
                Dosis = "850 mg",
                Frecuencia = "Cada 12 horas",
                Horario = "08:00, 20:00",
                FechaInicio = today.AddMonths(-18),
                Observaciones = "Tomar con comida. Revisar tolerancia digestiva.",
                Activo = true
            },
            new Medication
            {
                Id = Guid.Parse("dddddddd-0000-0000-0000-000000000002"),
                PacienteId = Patient2Id,
                Nombre = "Losartán",
                Dosis = "50 mg",
                Frecuencia = "Cada 24 horas",
                Horario = "09:00",
                FechaInicio = today.AddYears(-1),
                Observaciones = "Controlar tensión y función renal.",
                Activo = true
            },
            new Medication
            {
                Id = Guid.Parse("dddddddd-0000-0000-0000-000000000003"),
                PacienteId = Patient2Id,
                Nombre = "Amlodipino",
                Dosis = "5 mg",
                Frecuencia = "Cada 24 horas",
                Horario = "21:00",
                FechaInicio = today.AddMonths(-5),
                Observaciones = "Añadido por control tensional insuficiente.",
                Activo = true
            }
        ]);

        await measurementRepository.SaveAllAsync(BuildMeasurements(today));

        await medicalNoteRepository.SaveAllAsync(
        [
            new MedicalNote
            {
                Id = Guid.Parse("eeeeeeee-0000-0000-0000-000000000001"),
                PacienteId = Patient1Id,
                DoctorId = DoctorId,
                Titulo = "Revisión metabólica",
                Contenido = "Glucemias estables. Mantener tratamiento y reforzar actividad física.",
                VisibleParaPaciente = true,
                FechaHora = today.AddDays(-12).AddHours(10)
            },
            new MedicalNote
            {
                Id = Guid.Parse("eeeeeeee-0000-0000-0000-000000000002"),
                PacienteId = Patient1Id,
                DoctorId = DoctorId,
                Titulo = "Nota interna",
                Contenido = "Valorar HbA1c en próxima analítica.",
                VisibleParaPaciente = false,
                FechaHora = today.AddDays(-5).AddHours(12)
            },
            new MedicalNote
            {
                Id = Guid.Parse("eeeeeeee-0000-0000-0000-000000000003"),
                PacienteId = Patient2Id,
                DoctorId = DoctorId,
                Titulo = "Control de presión arterial",
                Contenido = "Tensión mejor controlada tras ajuste terapéutico. Mantener controles domiciliarios.",
                VisibleParaPaciente = true,
                FechaHora = today.AddDays(-10).AddHours(9)
            }
        ]);

        await reportRepository.SaveAllAsync(
        [
            new Report
            {
                Id = Guid.Parse("ffffffff-0000-0000-0000-000000000001"),
                PacienteId = Patient1Id,
                GeneradoPorUsuarioId = DoctorId,
                FechaGeneracion = today.AddDays(-3).AddHours(11),
                FechaInicioPeriodo = today.AddMonths(-1),
                FechaFinPeriodo = today,
                Resumen = "Paciente con diabetes tipo 2 activa y antecedente de asma superada. Glucemias recientes dentro de objetivo para demo."
            },
            new Report
            {
                Id = Guid.Parse("ffffffff-0000-0000-0000-000000000002"),
                PacienteId = Patient2Id,
                GeneradoPorUsuarioId = DoctorId,
                FechaGeneracion = today.AddDays(-2).AddHours(13),
                FechaInicioPeriodo = today.AddMonths(-1),
                FechaFinPeriodo = today,
                Resumen = "Paciente con hipertensión arterial y enfermedad renal crónica activas. Tensión en descenso tras ajuste de medicación."
            }
        ]);
    }

    /// <summary>
    /// Crea usuarios demo con el mismo hash de contrasena que usa el login real.
    /// </summary>
    private static List<User> BuildUsers()
    {
        var createdAt = new DateTime(2026, 1, 1, 9, 0, 0);
        return
        [
            new User
            {
                Id = AdminId,
                Nombre = "Admin",
                Apellidos = "Demo",
                Email = "admin@meditrack.demo",
                NombreUsuario = "admin",
                PasswordHash = PasswordHelper.HashPassword("Admin123!"),
                Rol = UserRole.Administrador,
                Activo = true,
                FechaCreacion = createdAt
            },
            new User
            {
                Id = DoctorId,
                Nombre = "Doctor",
                Apellidos = "Demo",
                Email = "doctor@meditrack.demo",
                NombreUsuario = "doctor",
                PasswordHash = PasswordHelper.HashPassword("Doctor123!"),
                Rol = UserRole.Doctor,
                Activo = true,
                FechaCreacion = createdAt
            },
            new User
            {
                Id = Patient1Id,
                Nombre = "Paciente",
                Apellidos = "Uno",
                Email = "paciente1@meditrack.demo",
                NombreUsuario = "paciente1",
                PasswordHash = PasswordHelper.HashPassword("Paciente123!"),
                Rol = UserRole.Paciente,
                Activo = true,
                FechaCreacion = createdAt
            },
            new User
            {
                Id = Patient2Id,
                Nombre = "Paciente",
                Apellidos = "Dos",
                Email = "paciente2@meditrack.demo",
                NombreUsuario = "paciente2",
                PasswordHash = PasswordHelper.HashPassword("Paciente123!"),
                Rol = UserRole.Paciente,
                Activo = true,
                FechaCreacion = createdAt
            }
        ];
    }

    /// <summary>
    /// Crea el catalogo base de enfermedades, manteniendo identificadores fijos para evitar duplicados.
    /// </summary>
    private static List<ChronicDisease> BuildDiseases() =>
    [
        new ChronicDisease
        {
            Id = DiabetesId,
            Nombre = AppConstants.BaseDiseases[0],
            Descripcion = "Alteración metabólica caracterizada por resistencia a la insulina."
        },
        new ChronicDisease
        {
            Id = HypertensionId,
            Nombre = AppConstants.BaseDiseases[1],
            Descripcion = "Elevación persistente de la presión arterial."
        },
        new ChronicDisease
        {
            Id = AsthmaId,
            Nombre = AppConstants.BaseDiseases[2],
            Descripcion = "Enfermedad inflamatoria crónica de las vías respiratorias."
        },
        new ChronicDisease
        {
            Id = KidneyDiseaseId,
            Nombre = AppConstants.BaseDiseases[3],
            Descripcion = "Pérdida progresiva de la función renal."
        }
    ];

    /// <summary>
    /// Genera mediciones verosimiles y suficientes para probar historiales y graficas.
    /// </summary>
    private static List<Measurement> BuildMeasurements(DateTime today) =>
    [
        new Measurement { Id = Guid.Parse("99999999-0000-0000-0000-000000000001"), PacienteId = Patient1Id, TipoMedicion = MeasurementType.Glucosa, Valor = 132, Unidad = "mg/dL", FechaHora = today.AddDays(-21).AddHours(8), Observaciones = "Ayunas" },
        new Measurement { Id = Guid.Parse("99999999-0000-0000-0000-000000000002"), PacienteId = Patient1Id, TipoMedicion = MeasurementType.Glucosa, Valor = 126, Unidad = "mg/dL", FechaHora = today.AddDays(-14).AddHours(8), Observaciones = "Ayunas" },
        new Measurement { Id = Guid.Parse("99999999-0000-0000-0000-000000000003"), PacienteId = Patient1Id, TipoMedicion = MeasurementType.Glucosa, Valor = 118, Unidad = "mg/dL", FechaHora = today.AddDays(-7).AddHours(8), Observaciones = "Ayunas" },
        new Measurement { Id = Guid.Parse("99999999-0000-0000-0000-000000000004"), PacienteId = Patient1Id, TipoMedicion = MeasurementType.Peso, Valor = 68, Unidad = "kg", FechaHora = today.AddDays(-21).AddHours(9), Observaciones = "Control semanal" },
        new Measurement { Id = Guid.Parse("99999999-0000-0000-0000-000000000005"), PacienteId = Patient1Id, TipoMedicion = MeasurementType.Peso, Valor = 67, Unidad = "kg", FechaHora = today.AddDays(-7).AddHours(9), Observaciones = "Control semanal" },
        new Measurement { Id = Guid.Parse("99999999-0000-0000-0000-000000000006"), PacienteId = Patient2Id, TipoMedicion = MeasurementType.PresionSistolica, Valor = 148, Unidad = "mmHg", FechaHora = today.AddDays(-18).AddHours(8), Observaciones = "Antes de medicación" },
        new Measurement { Id = Guid.Parse("99999999-0000-0000-0000-000000000007"), PacienteId = Patient2Id, TipoMedicion = MeasurementType.PresionDiastolica, Valor = 92, Unidad = "mmHg", FechaHora = today.AddDays(-18).AddHours(8), Observaciones = "Antes de medicación" },
        new Measurement { Id = Guid.Parse("99999999-0000-0000-0000-000000000008"), PacienteId = Patient2Id, TipoMedicion = MeasurementType.PresionSistolica, Valor = 136, Unidad = "mmHg", FechaHora = today.AddDays(-5).AddHours(8), Observaciones = "Control domiciliario" },
        new Measurement { Id = Guid.Parse("99999999-0000-0000-0000-000000000009"), PacienteId = Patient2Id, TipoMedicion = MeasurementType.PresionDiastolica, Valor = 84, Unidad = "mmHg", FechaHora = today.AddDays(-5).AddHours(8), Observaciones = "Control domiciliario" },
        new Measurement { Id = Guid.Parse("99999999-0000-0000-0000-000000000010"), PacienteId = Patient2Id, TipoMedicion = MeasurementType.Peso, Valor = 84, Unidad = "kg", FechaHora = today.AddDays(-5).AddHours(9), Observaciones = "Sin edemas" }
    ];
}
