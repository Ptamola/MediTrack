using MediTrack.Core.Interfaces.Repositories;
using MediTrack.Core.Models;
using MediTrack.Data.Config;

namespace MediTrack.Data.Repositories;

public class UserRepository(StoragePaths storagePaths)
    : JsonRepositoryBase<User>(storagePaths, "usuarios.json"), IUserRepository
{
}
