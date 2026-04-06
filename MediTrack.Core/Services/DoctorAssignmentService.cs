using MediTrack.Core.DTOs;
using MediTrack.Core.Enums;
using MediTrack.Core.Interfaces.Repositories;
using MediTrack.Core.Interfaces.Services;
using MediTrack.Core.Models;

namespace MediTrack.Core.Services;

public class DoctorAssignmentService(
    IDoctorPatientAssignmentRepository assignmentRepository,
    IUserRepository userRepository) : IDoctorAssignmentService
{
    public Task<List<DoctorPatientAssignment>> GetAllAsync() => assignmentRepository.GetAllAsync();

    public async Task<List<User>> GetDoctorsForPatientAsync(Guid patientId)
    {
        var currentDoctorIds = GetCurrentAssignments(await assignmentRepository.GetAllAsync())
            .Where(a => a.PacienteId == patientId)
            .Select(a => a.DoctorId)
            .ToHashSet();

        return (await userRepository.GetAllAsync())
            .Where(u => u.Rol == UserRole.Doctor && currentDoctorIds.Contains(u.Id))
            .OrderBy(u => u.Apellidos)
            .ThenBy(u => u.Nombre)
            .ToList();
    }

    public async Task<List<User>> GetPreviousDoctorsForPatientAsync(Guid patientId)
    {
        var assignments = await assignmentRepository.GetAllAsync();
        var currentAssignmentId = GetCurrentAssignments(assignments)
            .Where(a => a.PacienteId == patientId)
            .Select(a => a.Id)
            .FirstOrDefault();
        var users = await userRepository.GetAllAsync();

        return assignments
            .Where(a => a.PacienteId == patientId && a.Id != currentAssignmentId)
            .OrderByDescending(a => a.FechaFinAsignacion ?? a.FechaAsignacion)
            .GroupBy(a => a.DoctorId)
            .Select(group => users.FirstOrDefault(u => u.Id == group.Key))
            .Where(user => user is not null)
            .Cast<User>()
            .ToList();
    }

    public async Task<List<User>> GetPatientsForDoctorAsync(Guid doctorId)
    {
        var currentPatientIds = GetCurrentAssignments(await assignmentRepository.GetAllAsync())
            .Where(a => a.DoctorId == doctorId)
            .Select(a => a.PacienteId)
            .ToHashSet();

        return (await userRepository.GetAllAsync())
            .Where(u => u.Rol == UserRole.Paciente && currentPatientIds.Contains(u.Id))
            .OrderBy(u => u.Apellidos)
            .ThenBy(u => u.Nombre)
            .ToList();
    }

    public async Task<OperationResult> AssignAsync(Guid doctorId, Guid patientId)
    {
        var assignments = await assignmentRepository.GetAllAsync();
        var currentAssignments = GetCurrentAssignments(assignments).Where(a => a.PacienteId == patientId).ToList();

        if (currentAssignments.Any(a => a.DoctorId == doctorId))
        {
            return OperationResult.Fail("Ese doctor ya es el asignado actualmente a este paciente.");
        }

        foreach (var assignment in currentAssignments)
        {
            assignment.Activa = false;
            assignment.FechaFinAsignacion = DateTime.Today;
        }

        assignments.Add(new DoctorPatientAssignment
        {
            DoctorId = doctorId,
            PacienteId = patientId,
            FechaAsignacion = DateTime.Today,
            Activa = true
        });

        await assignmentRepository.SaveAllAsync(assignments);

        return currentAssignments.Count == 0
            ? OperationResult.Ok("Asignación registrada correctamente.")
            : OperationResult.Ok("Asignación actualizada y doctor anterior archivado en el historial.");
    }

    private static List<DoctorPatientAssignment> GetCurrentAssignments(IEnumerable<DoctorPatientAssignment> assignments)
    {
        return assignments
            .Where(a => a.Activa)
            .GroupBy(a => a.PacienteId)
            .Select(group => group
                .OrderByDescending(a => a.FechaAsignacion)
                .ThenByDescending(a => a.Id)
                .First())
            .ToList();
    }
}
