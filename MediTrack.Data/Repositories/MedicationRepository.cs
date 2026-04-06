using MediTrack.Core.Interfaces.Repositories;
using MediTrack.Core.Models;
using MediTrack.Data.Config;

namespace MediTrack.Data.Repositories;

public class MedicationRepository(StoragePaths storagePaths)
    : JsonRepositoryBase<Medication>(storagePaths, "medicamentos.json"), IMedicationRepository
{
}
