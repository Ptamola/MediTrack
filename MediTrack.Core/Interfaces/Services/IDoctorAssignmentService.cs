using MediTrack.Core.DTOs;
using MediTrack.Core.Models;

namespace MediTrack.Core.Interfaces.Services;

public interface IDoctorAssignmentService
{
    Task<List<DoctorPatientAssignment>> GetAllAsync();
    Task<List<User>> GetDoctorsForPatientAsync(Guid patientId);
    Task<List<User>> GetPreviousDoctorsForPatientAsync(Guid patientId);
    Task<List<User>> GetPatientsForDoctorAsync(Guid doctorId);
    Task<OperationResult> AssignAsync(Guid doctorId, Guid patientId);
}
