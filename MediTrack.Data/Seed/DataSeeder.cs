using MediTrack.Core.Constants;
using MediTrack.Core.Enums;
using MediTrack.Core.Helpers;
using MediTrack.Core.Interfaces.Repositories;
using MediTrack.Core.Models;

namespace MediTrack.Data.Seed;

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
    IDoctorPatientAssignmentRepository assignmentRepository)
{
    public async Task SeedAsync()
    {
        var existingUsers = await userRepository.GetAllAsync();
        if (existingUsers.Count > 0)
        {
            return;
        }

        var admin = new User
        {
            Nombre = "Pedro",
            Apellidos = "Tamola",
            Email = "admin@meditrack.local",
            NombreUsuario = "admin",
            PasswordHash = PasswordHelper.HashPassword("Admin123"),
            Rol = UserRole.Administrador,
            Activo = true
        };

        var doctor1 = new User
        {
            Nombre = "Carlos",
            Apellidos = "Ruiz",
            Email = "carlos.ruiz@meditrack.local",
            NombreUsuario = "doctor.carlos",
            PasswordHash = PasswordHelper.HashPassword("Doctor123"),
            Rol = UserRole.Doctor,
            Activo = true
        };

        var doctor2 = new User
        {
            Nombre = "Elena",
            Apellidos = "Santos",
            Email = "elena.santos@meditrack.local",
            NombreUsuario = "doctora.elena",
            PasswordHash = PasswordHelper.HashPassword("Doctor123"),
            Rol = UserRole.Doctor,
            Activo = true
        };

        var patients = new[]
        {
            new User
            {
                Nombre = "Ana",
                Apellidos = "GÃ³mez",
                Email = "ana.gomez@meditrack.local",
                NombreUsuario = "ana.gomez",
                PasswordHash = PasswordHelper.HashPassword("Paciente123"),
                Rol = UserRole.Paciente,
                Activo = true
            },
            new User
            {
                Nombre = "Miguel",
                Apellidos = "PÃ©rez",
                Email = "miguel.perez@meditrack.local",
                NombreUsuario = "miguel.perez",
                PasswordHash = PasswordHelper.HashPassword("Paciente123"),
                Rol = UserRole.Paciente,
                Activo = true
            },
            new User
            {
                Nombre = "LucÃ­a",
                Apellidos = "Navarro",
                Email = "lucia.navarro@meditrack.local",
                NombreUsuario = "lucia.navarro",
                PasswordHash = PasswordHelper.HashPassword("Paciente123"),
                Rol = UserRole.Paciente,
                Activo = true
            },
            new User
            {
                Nombre = "Javier",
                Apellidos = "Torres",
                Email = "javier.torres@meditrack.local",
                NombreUsuario = "javier.torres",
                PasswordHash = PasswordHelper.HashPassword("Paciente123"),
                Rol = UserRole.Paciente,
                Activo = true
            }
        };

        var users = new List<User> { admin, doctor1, doctor2 };
        users.AddRange(patients);
        await userRepository.SaveAllAsync(users);

        await administratorRepository.SaveAllAsync([new Administrator { IdUsuario = admin.Id }]);
        await doctorRepository.SaveAllAsync(
        [
            new Doctor { IdUsuario = doctor1.Id, NumeroColegiado = "COL-1001", Especialidad = "EndocrinologÃ­a" },
            new Doctor { IdUsuario = doctor2.Id, NumeroColegiado = "COL-1002", Especialidad = "NeumologÃ­a" }
        ]);

        var patientProfiles = new List<Patient>
        {
            new() { IdUsuario = patients[0].Id, FechaNacimiento = new DateTime(1978, 4, 12), Telefono = "600111222", Direccion = "Calle Mayor 10, Madrid", ObservacionesGenerales = "Controla glucosa diariamente." },
            new() { IdUsuario = patients[1].Id, FechaNacimiento = new DateTime(1965, 8, 3), Telefono = "600333444", Direccion = "Avenida Sol 22, Valencia", ObservacionesGenerales = "HipertensiÃ³n con seguimiento mensual." },
            new() { IdUsuario = patients[2].Id, FechaNacimiento = new DateTime(1989, 11, 19), Telefono = "600555666", Direccion = "Gran VÃ­a 55, Sevilla", ObservacionesGenerales = "Asma persistente moderada." },
            new() { IdUsuario = patients[3].Id, FechaNacimiento = new DateTime(1959, 2, 25), Telefono = "600777888", Direccion = "Plaza Norte 4, Bilbao", ObservacionesGenerales = "Enfermedad renal crÃ³nica controlada." }
        };
        await patientRepository.SaveAllAsync(patientProfiles);

        var diseases = new List<ChronicDisease>
        {
            new() { Nombre = AppConstants.BaseDiseases[0], Descripcion = "AlteraciÃ³n metabÃ³lica caracterizada por resistencia a la insulina." },
            new() { Nombre = AppConstants.BaseDiseases[1], Descripcion = "ElevaciÃ³n persistente de la presiÃ³n arterial." },
            new() { Nombre = AppConstants.BaseDiseases[2], Descripcion = "Enfermedad inflamatoria crÃ³nica de las vÃ­as respiratorias." },
            new() { Nombre = AppConstants.BaseDiseases[3], Descripcion = "PÃ©rdida progresiva de la funciÃ³n renal." }
        };
        await chronicDiseaseRepository.SaveAllAsync(diseases);

        await patientDiseaseRepository.SaveAllAsync(
        [
            new PatientDisease { PacienteId = patients[0].Id, EnfermedadId = diseases[0].Id, FechaDiagnostico = DateTime.Today.AddYears(-5), Observaciones = "Control con dieta y metformina." },
            new PatientDisease { PacienteId = patients[1].Id, EnfermedadId = diseases[1].Id, FechaDiagnostico = DateTime.Today.AddYears(-8), Observaciones = "Control con IECA." },
            new PatientDisease { PacienteId = patients[2].Id, EnfermedadId = diseases[2].Id, FechaDiagnostico = DateTime.Today.AddYears(-12), Observaciones = "Empeora en primavera." },
            new PatientDisease { PacienteId = patients[3].Id, EnfermedadId = diseases[3].Id, FechaDiagnostico = DateTime.Today.AddYears(-3), Observaciones = "Control nefrolÃ³gico trimestral." }
        ]);

        await assignmentRepository.SaveAllAsync(
        [
            new DoctorPatientAssignment { DoctorId = doctor1.Id, PacienteId = patients[0].Id, FechaAsignacion = DateTime.Today.AddMonths(-6), Activa = true },
            new DoctorPatientAssignment { DoctorId = doctor1.Id, PacienteId = patients[1].Id, FechaAsignacion = DateTime.Today.AddMonths(-4), Activa = true },
            new DoctorPatientAssignment { DoctorId = doctor2.Id, PacienteId = patients[2].Id, FechaAsignacion = DateTime.Today.AddMonths(-7), Activa = true },
            new DoctorPatientAssignment { DoctorId = doctor2.Id, PacienteId = patients[3].Id, FechaAsignacion = DateTime.Today.AddMonths(-2), Activa = true }
        ]);

        await medicationRepository.SaveAllAsync(
        [
            new Medication { PacienteId = patients[0].Id, Nombre = "Metformina", Dosis = "850 mg", Frecuencia = "Cada 12 horas", Horario = "08:00,20:00", FechaInicio = DateTime.Today.AddMonths(-10), Observaciones = "Tomar con comida.", Activo = true },
            new Medication { PacienteId = patients[1].Id, Nombre = "Enalapril", Dosis = "10 mg", Frecuencia = "Cada 24 horas", Horario = "09:00", FechaInicio = DateTime.Today.AddMonths(-14), Observaciones = "Controlar tensiÃ³n.", Activo = true },
            new Medication { PacienteId = patients[2].Id, Nombre = "Salbutamol", Dosis = "2 inhalaciones", Frecuencia = "A demanda", Horario = "08:00,22:00", FechaInicio = DateTime.Today.AddMonths(-5), Observaciones = "Llevar inhalador siempre.", Activo = true },
            new Medication { PacienteId = patients[3].Id, Nombre = "LosartÃ¡n", Dosis = "50 mg", Frecuencia = "Cada 24 horas", Horario = "10:00", FechaInicio = DateTime.Today.AddMonths(-8), Observaciones = "Seguimiento renal y tensiÃ³n.", Activo = true }
        ]);

        await measurementRepository.SaveAllAsync(BuildMeasurements(patients));

        await medicalNoteRepository.SaveAllAsync(
        [
            new MedicalNote { PacienteId = patients[0].Id, DoctorId = doctor1.Id, Titulo = "RevisiÃ³n trimestral", Contenido = "Buen control glucÃ©mico, mantener tratamiento.", VisibleParaPaciente = true, FechaHora = DateTime.Now.AddDays(-15) },
            new MedicalNote { PacienteId = patients[1].Id, DoctorId = doctor1.Id, Titulo = "PresiÃ³n arterial", Contenido = "Se recomienda reducir sal y continuar ejercicio suave.", VisibleParaPaciente = true, FechaHora = DateTime.Now.AddDays(-10) },
            new MedicalNote { PacienteId = patients[2].Id, DoctorId = doctor2.Id, Titulo = "Asma estable", Contenido = "Sin crisis recientes, revisar uso de inhalador.", VisibleParaPaciente = true, FechaHora = DateTime.Now.AddDays(-5) },
            new MedicalNote { PacienteId = patients[3].Id, DoctorId = doctor2.Id, Titulo = "Control renal", Contenido = "Mantener hidrataciÃ³n y repetir analÃ­tica en dos meses.", VisibleParaPaciente = true, FechaHora = DateTime.Now.AddDays(-8) }
        ]);
    }

    private static List<Measurement> BuildMeasurements(User[] patients)
    {
        var items = new List<Measurement>();

        foreach (var patient in patients)
        {
            for (var i = 10; i >= 1; i--)
            {
                var date = DateTime.Now.AddDays(-i * 3);
                items.Add(new Measurement
                {
                    PacienteId = patient.Id,
                    TipoMedicion = MeasurementType.Peso,
                    Valor = 60 + Random.Shared.Next(0, 25),
                    Unidad = "kg",
                    FechaHora = date,
                    Observaciones = "Registro domiciliario"
                });
            }
        }

        items.AddRange(
        [
            new Measurement { PacienteId = patients[0].Id, TipoMedicion = MeasurementType.Glucosa, Valor = 118, Unidad = "mg/dL", FechaHora = DateTime.Now.AddDays(-7), Observaciones = "Antes del desayuno" },
            new Measurement { PacienteId = patients[0].Id, TipoMedicion = MeasurementType.Glucosa, Valor = 126, Unidad = "mg/dL", FechaHora = DateTime.Now.AddDays(-3), Observaciones = "Ayunas" },
            new Measurement { PacienteId = patients[1].Id, TipoMedicion = MeasurementType.PresionSistolica, Valor = 132, Unidad = "mmHg", FechaHora = DateTime.Now.AddDays(-6), Observaciones = "Control matutino" },
            new Measurement { PacienteId = patients[1].Id, TipoMedicion = MeasurementType.PresionDiastolica, Valor = 84, Unidad = "mmHg", FechaHora = DateTime.Now.AddDays(-6), Observaciones = "Control matutino" },
            new Measurement { PacienteId = patients[2].Id, TipoMedicion = MeasurementType.SaturacionOxigeno, Valor = 97, Unidad = "%", FechaHora = DateTime.Now.AddDays(-4), Observaciones = "Sin disnea" },
            new Measurement { PacienteId = patients[2].Id, TipoMedicion = MeasurementType.FrecuenciaCardiaca, Valor = 76, Unidad = "lpm", FechaHora = DateTime.Now.AddDays(-4), Observaciones = "Reposo" },
            new Measurement { PacienteId = patients[3].Id, TipoMedicion = MeasurementType.Temperatura, Valor = 36.5m, Unidad = "Â°C", FechaHora = DateTime.Now.AddDays(-2), Observaciones = "Control general" },
            new Measurement { PacienteId = patients[3].Id, TipoMedicion = MeasurementType.SintomasPersonalizados, Valor = 1, Unidad = "texto", FechaHora = DateTime.Now.AddDays(-2), Observaciones = "Leve cansancio por la tarde" }
        ]);

        return items;
    }
}

