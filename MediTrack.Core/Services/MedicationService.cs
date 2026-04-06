using MediTrack.Core.DTOs;
using MediTrack.Core.Helpers;
using MediTrack.Core.Interfaces.Repositories;
using MediTrack.Core.Interfaces.Services;
using MediTrack.Core.Models;

namespace MediTrack.Core.Services;

public class MedicationService(IMedicationRepository medicationRepository) : IMedicationService
{
    public async Task<List<Medication>> GetByPatientAsync(Guid patientId) =>
        (await medicationRepository.GetAllAsync())
        .Where(m => m.PacienteId == patientId)
        .OrderByDescending(m => m.Activo)
        .ThenBy(m => m.Nombre)
        .ToList();

    public async Task<List<MedicationReminder>> GetRemindersAsync(Guid patientId)
    {
        var medications = await GetByPatientAsync(patientId);
        return ReminderHelper.BuildReminders(medications);
    }

    public async Task<OperationResult> SaveAsync(Medication medication)
    {
        var validation = ValidationHelper.ValidateMedication(medication);
        if (!validation.Success)
        {
            return validation;
        }

        var medications = await medicationRepository.GetAllAsync();
        var existing = medications.FirstOrDefault(m => m.Id == medication.Id);
        if (existing == null)
        {
            medication.Id = Guid.NewGuid();
            medications.Add(medication);
        }
        else
        {
            existing.Nombre = medication.Nombre.Trim();
            existing.Dosis = medication.Dosis.Trim();
            existing.Frecuencia = medication.Frecuencia.Trim();
            existing.Horario = medication.Horario.Trim();
            existing.FechaInicio = medication.FechaInicio;
            existing.FechaFin = medication.FechaFin;
            existing.Observaciones = medication.Observaciones.Trim();
            existing.Activo = medication.Activo;
        }

        await medicationRepository.SaveAllAsync(medications);
        return OperationResult.Ok("Medicamento guardado.");
    }

    public async Task<OperationResult> DeleteAsync(Guid medicationId)
    {
        var medications = await medicationRepository.GetAllAsync();
        var existing = medications.FirstOrDefault(m => m.Id == medicationId);
        if (existing == null)
        {
            return OperationResult.Fail("No se encontró el medicamento.");
        }

        medications.Remove(existing);
        await medicationRepository.SaveAllAsync(medications);
        return OperationResult.Ok("Medicamento eliminado.");
    }
}
