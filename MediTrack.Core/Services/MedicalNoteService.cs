using MediTrack.Core.DTOs;
using MediTrack.Core.Interfaces.Repositories;
using MediTrack.Core.Interfaces.Services;
using MediTrack.Core.Models;

namespace MediTrack.Core.Services;

public class MedicalNoteService(IMedicalNoteRepository medicalNoteRepository) : IMedicalNoteService
{
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
