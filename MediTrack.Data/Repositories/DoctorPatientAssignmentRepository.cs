using MediTrack.Core.Interfaces.Repositories;
using MediTrack.Core.Models;
using MediTrack.Data.Config;

namespace MediTrack.Data.Repositories;

public class DoctorPatientAssignmentRepository(StoragePaths storagePaths)
    : JsonRepositoryBase<DoctorPatientAssignment>(storagePaths, "asignaciones.json"), IDoctorPatientAssignmentRepository
{
}
