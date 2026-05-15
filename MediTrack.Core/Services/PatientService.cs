using MediTrack.Core.DTOs;
using MediTrack.Core.Interfaces.Repositories;
using MediTrack.Core.Interfaces.Services;
using MediTrack.Core.Models;

namespace MediTrack.Core.Services;

/// <summary>
/// Servicio de pacientes. Gestiona perfil ampliado y resumen de pacientes asignados a doctores.
/// </summary>
public class PatientService(
    IPatientRepository patientRepository,
    IUserRepository userRepository,
    IDoctorPatientAssignmentRepository assignmentRepository,
    IPatientDiseaseRepository patientDiseaseRepository,
    IChronicDiseaseRepository chronicDiseaseRepository) : IPatientService
{
    public Task<List<Patient>> GetAllAsync() => patientRepository.GetAllAsync();

    public async Task<Patient?> GetByUserIdAsync(Guid userId) =>
        (await patientRepository.GetAllAsync()).FirstOrDefault(p => p.IdUsuario == userId);

    /// <summary>
    /// Actualiza el perfil clinico ampliado del paciente, incluida la ruta relativa de la foto.
    /// </summary>
    public async Task<OperationResult> UpdateAsync(Patient patient)
    {
        var patients = await patientRepository.GetAllAsync();
        var existing = patients.FirstOrDefault(p => p.IdUsuario == patient.IdUsuario);
        if (existing == null)
        {
            return OperationResult.Fail("No se encontró el perfil del paciente.");
        }

        existing.FechaNacimiento = patient.FechaNacimiento;
        existing.Telefono = patient.Telefono.Trim();
        existing.Direccion = patient.Direccion.Trim();
        existing.ObservacionesGenerales = patient.ObservacionesGenerales.Trim();
        existing.DniNie = patient.DniNie.Trim();
        existing.Sexo = patient.Sexo.Trim();
        existing.GrupoSanguineo = patient.GrupoSanguineo.Trim();
        existing.AlturaCm = patient.AlturaCm;
        existing.PesoKg = patient.PesoKg;
        existing.Alergias = patient.Alergias.Trim();
        existing.AntecedentesMedicos = patient.AntecedentesMedicos.Trim();
        existing.ContactoEmergenciaNombre = patient.ContactoEmergenciaNombre.Trim();
        existing.ContactoEmergenciaTelefono = patient.ContactoEmergenciaTelefono.Trim();
        existing.SeguroMedico = patient.SeguroMedico.Trim();
        existing.NumeroTarjetaSanitaria = patient.NumeroTarjetaSanitaria.Trim();
        existing.FotoRuta = patient.FotoRuta.Trim();
        await patientRepository.SaveAllAsync(patients);
        return OperationResult.Ok("Perfil actualizado.");
    }

    /// <summary>
    /// Devuelve pacientes asignados al doctor teniendo en cuenta solo la asignacion activa mas reciente.
    /// </summary>
    public async Task<List<PatientSummaryDto>> GetAssignedPatientsSummaryAsync(Guid doctorId)
    {
        var assignments = await assignmentRepository.GetAllAsync();
        var patientIds = assignments
            .Where(a => a.Activa)
            .GroupBy(a => a.PacienteId)
            .Select(group => group.OrderByDescending(a => a.FechaAsignacion).ThenByDescending(a => a.Id).First())
            .Where(a => a.DoctorId == doctorId)
            .Select(a => a.PacienteId)
            .ToHashSet();
        var users = await userRepository.GetAllAsync();
        var patients = await patientRepository.GetAllAsync();
        var patientDiseases = await patientDiseaseRepository.GetAllAsync();
        var diseases = await chronicDiseaseRepository.GetAllAsync();

        return patients
            .Where(p => patientIds.Contains(p.IdUsuario))
            .Join(users, p => p.IdUsuario, u => u.Id, (p, u) => new { Profile = p, User = u })
            .Select(item =>
            {
                var diseaseNames = patientDiseases
                    .Where(pd => pd.PacienteId == item.Profile.IdUsuario)
                    .Join(diseases, pd => pd.EnfermedadId, d => d.Id, (_, d) => d.Nombre);

                return new PatientSummaryDto
                {
                    PacienteId = item.Profile.IdUsuario,
                    NombreCompleto = item.User.NombreCompleto,
                    Email = item.User.Email,
                    Telefono = item.Profile.Telefono,
                    Enfermedades = string.Join(", ", diseaseNames)
                };
            })
            .OrderBy(x => x.NombreCompleto)
            .ToList();
    }
}
