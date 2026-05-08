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
        item.Activa = true;
        item.FechaFin = null;
        relations.Add(item);
        await patientDiseaseRepository.SaveAllAsync(relations);
        return OperationResult.Ok("Enfermedad asignada correctamente.");
    }

    public async Task<OperationResult> AssignDiseaseByNameAsync(Guid patientId, string diseaseName, DateTime diagnosisDate, string observations)
    {
        var normalizedName = diseaseName.Trim();
        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            return OperationResult.Fail("Introduce o selecciona una enfermedad válida.");
        }

        var diseases = await chronicDiseaseRepository.GetAllAsync();
        var disease = diseases.FirstOrDefault(d => string.Equals(d.Nombre.Trim(), normalizedName, StringComparison.OrdinalIgnoreCase));
        if (disease == null)
        {
            disease = new ChronicDisease
            {
                Id = Guid.NewGuid(),
                Nombre = normalizedName,
                Descripcion = "Enfermedad creada desde la ficha clínica."
            };
            diseases.Add(disease);
            await chronicDiseaseRepository.SaveAllAsync(diseases);
        }

        return await AssignDiseaseAsync(new PatientDisease
        {
            PacienteId = patientId,
            EnfermedadId = disease.Id,
            FechaDiagnostico = diagnosisDate.Date,
            Observaciones = observations
        });
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
        existing.FechaFin = item.Activa ? null : item.FechaFin?.Date;
        existing.Activa = item.Activa;
        existing.Observaciones = item.Observaciones.Trim();
        await patientDiseaseRepository.SaveAllAsync(relations);
        return OperationResult.Ok("Enfermedad actualizada.");
    }
}
