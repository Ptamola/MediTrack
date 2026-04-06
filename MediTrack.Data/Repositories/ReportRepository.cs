using MediTrack.Core.Interfaces.Repositories;
using MediTrack.Core.Models;
using MediTrack.Data.Config;

namespace MediTrack.Data.Repositories;

public class ReportRepository(StoragePaths storagePaths)
    : JsonRepositoryBase<Report>(storagePaths, "informes.json"), IReportRepository
{
}
