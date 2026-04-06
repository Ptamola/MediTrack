using MediTrack.Core.Interfaces.Repositories;
using MediTrack.Core.Models;
using MediTrack.Data.Config;

namespace MediTrack.Data.Repositories;

public class DoctorRepository(StoragePaths storagePaths)
    : JsonRepositoryBase<Doctor>(storagePaths, "doctores.json"), IDoctorRepository
{
}
