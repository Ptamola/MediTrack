using MediTrack.Core.DTOs;
using MediTrack.Core.Helpers;
using MediTrack.Core.Interfaces.Repositories;
using MediTrack.Core.Interfaces.Services;
using MediTrack.Core.Models;

namespace MediTrack.Core.Services;

/// <summary>
/// Servicio de mediciones clinicas. Aplica filtros por paciente, tipo y fechas.
/// </summary>
public class MeasurementService(IMeasurementRepository measurementRepository) : IMeasurementService
{
    /// <summary>
    /// Consulta mediciones con filtros opcionales usados por tablas, informes y graficas.
    /// </summary>
    public async Task<List<Measurement>> GetByPatientAsync(Guid patientId, Enums.MeasurementType? type = null, DateTime? from = null, DateTime? to = null)
    {
        var query = (await measurementRepository.GetAllAsync())
            .Where(m => m.PacienteId == patientId);

        if (type.HasValue)
        {
            query = query.Where(m => m.TipoMedicion == type.Value);
        }

        if (from.HasValue)
        {
            query = query.Where(m => m.FechaHora >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(m => m.FechaHora <= to.Value);
        }

        return query.OrderByDescending(m => m.FechaHora).ToList();
    }

    public async Task<List<Measurement>> GetLatestAsync(Guid patientId, int count = 5) =>
        (await GetByPatientAsync(patientId)).Take(count).ToList();

    /// <summary>
    /// Guarda una medicion asignando automaticamente la unidad segun el tipo.
    /// </summary>
    public async Task<OperationResult> SaveAsync(Measurement measurement)
    {
        measurement.Unidad = MeasurementHelper.GetUnit(measurement.TipoMedicion);
        var validation = ValidationHelper.ValidateMeasurement(measurement);
        if (!validation.Success)
        {
            return validation;
        }

        var items = await measurementRepository.GetAllAsync();
        measurement.Id = Guid.NewGuid();
        items.Add(measurement);
        await measurementRepository.SaveAllAsync(items);
        return OperationResult.Ok("Medición registrada correctamente.");
    }
}
