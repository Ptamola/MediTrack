using MediTrack.Core.DTOs;
using MediTrack.Core.Models;

namespace MediTrack.Core.Interfaces.Services;

public interface IMedicalNoteService
{
    Task<List<MedicalNote>> GetByPatientAsync(Guid patientId, bool onlyVisibleForPatient);
    Task<OperationResult> SaveAsync(MedicalNote note);
}
