using MediTrack.Core.Interfaces.Repositories;
using MediTrack.Core.Models;
using MediTrack.Data.Config;

namespace MediTrack.Data.Repositories;

public class MedicalNoteRepository(StoragePaths storagePaths)
    : JsonRepositoryBase<MedicalNote>(storagePaths, "notas_medicas.json"), IMedicalNoteRepository
{
}
