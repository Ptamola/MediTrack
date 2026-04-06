using MediTrack.Core.DTOs;
using MediTrack.Core.Models;

namespace MediTrack.Core.Interfaces.Services;

public interface IPatientService
{
    Task<List<Patient>> GetAllAsync();
    Task<Patient?> GetByUserIdAsync(Guid userId);
    Task<OperationResult> UpdateAsync(Patient patient);
    Task<List<PatientSummaryDto>> GetAssignedPatientsSummaryAsync(Guid doctorId);
}
