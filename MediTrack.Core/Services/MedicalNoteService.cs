using MediTrack.Core.DTOs;
using MediTrack.Core.Interfaces.Repositories;
using MediTrack.Core.Interfaces.Services;
using MediTrack.Core.Models;

namespace MediTrack.Core.Services;

/// <summary>
/// Servicio de notas medicas. Controla que el paciente solo vea notas marcadas como visibles.
/// </summary>
public class MedicalNoteService(IMedicalNoteRepository medicalNoteRepository) : IMedicalNoteService
{
    /// <summary>
    /// Obtiene notas de un paciente, filtrando por visibilidad cuando se consulta desde el rol paciente.
    /// </summary>
    public async Task<List<MedicalNote>> GetByPatientAsync(Guid patientId, bool onlyVisibleForPatient)
    {
        var query = (await medicalNoteRepository.GetAllAsync())
            .Where(n => n.PacienteId == patientId);

        if (onlyVisibleForPatient)
        {
            query = query.Where(n => n.VisibleParaPaciente);
        }

        return query.OrderByDescending(n => n.FechaHora).ToList();
    }

    /// <summary>
    /// Crea una nota medica con fecha actual y visibilidad indicada por el doctor.
    /// </summary>
    public async Task<OperationResult> SaveAsync(MedicalNote note)
    {
        if (string.IsNullOrWhiteSpace(note.Titulo) || string.IsNullOrWhiteSpace(note.Contenido))
        {
            return OperationResult.Fail("El título y el contenido de la nota son obligatorios.");
        }

        var notes = await medicalNoteRepository.GetAllAsync();
        note.Id = Guid.NewGuid();
        note.FechaHora = DateTime.Now;
        notes.Add(note);
        await medicalNoteRepository.SaveAllAsync(notes);
        return OperationResult.Ok("Nota médica guardada.");
    }
}
