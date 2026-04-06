using MediTrack.Core.Interfaces.Repositories;
using MediTrack.Core.Models;
using MediTrack.Data.Config;

namespace MediTrack.Data.Repositories;

public class MeasurementRepository(StoragePaths storagePaths)
    : JsonRepositoryBase<Measurement>(storagePaths, "mediciones.json"), IMeasurementRepository
{
}
