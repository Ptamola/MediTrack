using MediTrack.Core.Interfaces.Repositories;
using MediTrack.Core.Models;
using MediTrack.Data.Config;
using MySqlConnector;

namespace MediTrack.Data.Repositories.MySql;

public class ReportRepositoryMySql(DatabaseConnectionFactory connectionFactory)
    : MySqlRepositoryBase<Report>(connectionFactory), IReportRepository
{
    protected override string SelectSql => """
        SELECT Id, PacienteId, GeneradoPorUsuarioId, FechaGeneracion, FechaInicioPeriodo, FechaFinPeriodo, Resumen, RutaPdf
        FROM informes
        ORDER BY FechaGeneracion DESC;
        """;

    protected override string InsertSql => """
        INSERT INTO informes (Id, PacienteId, GeneradoPorUsuarioId, FechaGeneracion, FechaInicioPeriodo, FechaFinPeriodo, Resumen, RutaPdf)
        VALUES (@Id, @PacienteId, @GeneradoPorUsuarioId, @FechaGeneracion, @FechaInicioPeriodo, @FechaFinPeriodo, @Resumen, @RutaPdf)
        ON DUPLICATE KEY UPDATE
            PacienteId = VALUES(PacienteId),
            GeneradoPorUsuarioId = VALUES(GeneradoPorUsuarioId),
            FechaGeneracion = VALUES(FechaGeneracion),
            FechaInicioPeriodo = VALUES(FechaInicioPeriodo),
            FechaFinPeriodo = VALUES(FechaFinPeriodo),
            Resumen = VALUES(Resumen),
            RutaPdf = VALUES(RutaPdf);
        """;

    protected override Report Map(MySqlDataReader reader) => new()
    {
        Id = GetGuid(reader, "Id"),
        PacienteId = GetGuid(reader, "PacienteId"),
        GeneradoPorUsuarioId = GetGuid(reader, "GeneradoPorUsuarioId"),
        FechaGeneracion = GetDateTime(reader, "FechaGeneracion"),
        FechaInicioPeriodo = GetDateTime(reader, "FechaInicioPeriodo"),
        FechaFinPeriodo = GetDateTime(reader, "FechaFinPeriodo"),
        Resumen = GetString(reader, "Resumen"),
        RutaPdf = GetString(reader, "RutaPdf")
    };

    protected override void AddInsertParameters(MySqlCommand command, Report item)
    {
        AddParameter(command, "@Id", item.Id.ToString());
        AddParameter(command, "@PacienteId", item.PacienteId.ToString());
        AddParameter(command, "@GeneradoPorUsuarioId", item.GeneradoPorUsuarioId.ToString());
        AddParameter(command, "@FechaGeneracion", item.FechaGeneracion);
        AddParameter(command, "@FechaInicioPeriodo", item.FechaInicioPeriodo.Date);
        AddParameter(command, "@FechaFinPeriodo", item.FechaFinPeriodo.Date);
        AddParameter(command, "@Resumen", item.Resumen);
        AddParameter(command, "@RutaPdf", item.RutaPdf);
    }
}
