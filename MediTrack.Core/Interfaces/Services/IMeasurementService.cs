using MediTrack.Core.DTOs;
using MediTrack.Core.Enums;
using MediTrack.Core.Models;

namespace MediTrack.Core.Interfaces.Services;

public interface IMeasurementService
{
    Task<List<Measurement>> GetByPatientAsync(Guid patientId, MeasurementType? type = null, DateTime? from = null, DateTime? to = null);
    Task<List<Measurement>> GetLatestAsync(Guid patientId, int count = 5);
    Task<OperationResult> SaveAsync(Measurement measurement);
}
