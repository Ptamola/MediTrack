using MediTrack.Core.Interfaces.Repositories;
using MediTrack.Core.Models;
using MediTrack.Data.Config;

namespace MediTrack.Data.Repositories;

public class PatientDiseaseRepository(StoragePaths storagePaths)
    : JsonRepositoryBase<PatientDisease>(storagePaths, "paciente_enfermedades.json"), IPatientDiseaseRepository
{
}
