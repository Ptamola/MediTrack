-- MediTrack - Consultas de comprobación

SHOW DATABASES;

USE meditrak_db;

SHOW TABLES;

SELECT COUNT(*) AS total_usuarios FROM usuarios;
SELECT COUNT(*) AS total_pacientes FROM pacientes;
SELECT COUNT(*) AS total_doctores FROM doctores;
SELECT COUNT(*) AS total_enfermedades FROM enfermedades_cronicas;
SELECT COUNT(*) AS total_medicaciones FROM medicaciones;
SELECT COUNT(*) AS total_mediciones FROM mediciones;
SELECT COUNT(*) AS total_notas FROM notas_medicas;
SELECT COUNT(*) AS total_informes FROM informes;

SELECT
    paciente.Id AS PacienteId,
    CONCAT(paciente.Nombre, ' ', paciente.Apellidos) AS Paciente,
    CONCAT(doctor.Nombre, ' ', doctor.Apellidos) AS DoctorAsignado,
    dp.FechaAsignacion
FROM doctor_paciente dp
INNER JOIN usuarios paciente
    ON paciente.Id = dp.PacienteId
INNER JOIN usuarios doctor
    ON doctor.Id = dp.DoctorId
WHERE dp.Activa = 1
ORDER BY Paciente;
