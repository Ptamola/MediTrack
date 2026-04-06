using MediTrack.Core.Interfaces.Repositories;
using MediTrack.Core.Models;
using MediTrack.Data.Config;

namespace MediTrack.Data.Repositories;

public class ChronicDiseaseRepository(StoragePaths storagePaths)
    : JsonRepositoryBase<ChronicDisease>(storagePaths, "enfermedades.json"), IChronicDiseaseRepository
{
}
