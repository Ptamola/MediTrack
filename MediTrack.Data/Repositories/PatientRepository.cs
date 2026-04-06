using MediTrack.Core.Interfaces.Repositories;
using MediTrack.Core.Models;
using MediTrack.Data.Config;

namespace MediTrack.Data.Repositories;

public class PatientRepository(StoragePaths storagePaths)
    : JsonRepositoryBase<Patient>(storagePaths, "pacientes.json"), IPatientRepository
{
}
