using MediTrack.Core.Interfaces.Repositories;
using MediTrack.Core.Models;
using MediTrack.Data.Config;

namespace MediTrack.Data.Repositories;

public class AdministratorRepository(StoragePaths storagePaths)
    : JsonRepositoryBase<Administrator>(storagePaths, "administradores.json"), IAdministratorRepository
{
}
