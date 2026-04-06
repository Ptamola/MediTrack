using MediTrack.Core.DTOs;
using MediTrack.Core.Models;

namespace MediTrack.Core.Interfaces.Services;

public interface IMedicationService
{
    Task<List<Medication>> GetByPatientAsync(Guid patientId);
    Task<List<MedicationReminder>> GetRemindersAsync(Guid patientId);
    Task<OperationResult> SaveAsync(Medication medication);
    Task<OperationResult> DeleteAsync(Guid medicationId);
}
