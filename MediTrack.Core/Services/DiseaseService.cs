using MediTrack.Core.DTOs;
using MediTrack.Core.Interfaces.Repositories;
using MediTrack.Core.Interfaces.Services;
using MediTrack.Core.Models;

namespace MediTrack.Core.Services;

public class DiseaseService(
    IChronicDiseaseRepository chronicDiseaseRepository,
    IPatientDiseaseRepository patientDiseaseRepository) : IDiseaseService
{
    public async Task<List<ChronicDisease>> GetCatalogAsync() =>
        (await chronicDiseaseRepository.GetAllAsync()).OrderBy(d => d.Nombre).ToList();

    public async Task<List<PatientDisease>> GetPatientDiseasesAsync(Guid patientId) =>
        (await patientDiseaseRepository.GetAllAsync()).Where(d => d.PacienteId == patientId).OrderByDescending(d => d.FechaDiagnostico).ToList();

    public async Task<OperationResult> AssignDiseaseAsync(PatientDisease item)
    {
        var relations = await patientDiseaseRepository.GetAllAsync();
        if (relations.Any(r => r.PacienteId == item.PacienteId && r.EnfermedadId == item.EnfermedadId))
        {
            return OperationResult.Fail("La enfermedad ya está asociada al paciente.");
        }

        item.Id = Guid.NewGuid();
        relations.Add(item);
        await patientDiseaseRepository.SaveAllAsync(relations);
        return OperationResult.Ok("Enfermedad asignada correctamente.");
    }

    public async Task<OperationResult> UpdateAsync(PatientDisease item)
    {
        var relations = await patientDiseaseRepository.GetAllAsync();
        var existing = relations.FirstOrDefault(r => r.Id == item.Id);
        if (existing == null)
        {
            return OperationResult.Fail("No se encontró el registro de enfermedad.");
        }

        existing.FechaDiagnostico = item.FechaDiagnostico;
        existing.Observaciones = item.Observaciones.Trim();
        await patientDiseaseRepository.SaveAllAsync(relations);
        return OperationResult.Ok("Observaciones actualizadas.");
    }
}
