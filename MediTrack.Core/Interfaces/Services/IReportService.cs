using MediTrack.Core.DTOs;
using MediTrack.Core.Models;

namespace MediTrack.Core.Interfaces.Services;

public interface IReportService
{
    Task<List<Report>> GetByPatientAsync(Guid patientId);
    Task<GeneratedReportResult> GenerateAsync(Guid patientId, Guid generadoPorUsuarioId, DateTime fechaInicio, DateTime fechaFin, bool saveToHistory = true);
    Task<OperationResult> ExportPdfAsync(GeneratedReportResult reportResult, string outputFilePath);
}
