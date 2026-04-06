using MediTrack.Core.DTOs;
using MediTrack.Core.Models;

namespace MediTrack.Core.Interfaces.Services;

public interface IDiseaseService
{
    Task<List<ChronicDisease>> GetCatalogAsync();
    Task<List<PatientDisease>> GetPatientDiseasesAsync(Guid patientId);
    Task<OperationResult> AssignDiseaseAsync(PatientDisease item);
    Task<OperationResult> UpdateAsync(PatientDisease item);
}
