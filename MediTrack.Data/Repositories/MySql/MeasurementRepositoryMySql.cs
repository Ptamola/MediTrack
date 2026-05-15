using MediTrack.Core.Enums;
using MediTrack.Core.Interfaces.Repositories;
using MediTrack.Core.Models;
using MediTrack.Data.Config;
using MySqlConnector;

namespace MediTrack.Data.Repositories.MySql;

/// <summary>
/// Repositorio MySQL de mediciones clinicas registradas por paciente.
/// </summary>
public class MeasurementRepositoryMySql(DatabaseConnectionFactory connectionFactory)
    : MySqlRepositoryBase<Measurement>(connectionFactory), IMeasurementRepository
{
    protected override string SelectSql => """
        SELECT Id, PacienteId, TipoMedicion, Valor, Unidad, FechaHora, Observaciones
        FROM mediciones
        ORDER BY FechaHora DESC;
        """;

    protected override string InsertSql => """
        INSERT INTO mediciones (Id, PacienteId, TipoMedicion, Valor, Unidad, FechaHora, Observaciones)
        VALUES (@Id, @PacienteId, @TipoMedicion, @Valor, @Unidad, @FechaHora, @Observaciones)
        ON DUPLICATE KEY UPDATE
            PacienteId = VALUES(PacienteId),
            TipoMedicion = VALUES(TipoMedicion),
            Valor = VALUES(Valor),
            Unidad = VALUES(Unidad),
            FechaHora = VALUES(FechaHora),
            Observaciones = VALUES(Observaciones);
        """;

    protected override Measurement Map(MySqlDataReader reader) => new()
    {
        Id = GetGuid(reader, "Id"),
        PacienteId = GetGuid(reader, "PacienteId"),
        TipoMedicion = (MeasurementType)GetInt32(reader, "TipoMedicion"),
        Valor = GetDecimal(reader, "Valor"),
        Unidad = GetString(reader, "Unidad"),
        FechaHora = GetDateTime(reader, "FechaHora"),
        Observaciones = GetString(reader, "Observaciones")
    };

    protected override void AddInsertParameters(MySqlCommand command, Measurement item)
    {
        AddParameter(command, "@Id", item.Id.ToString());
        AddParameter(command, "@PacienteId", item.PacienteId.ToString());
        AddParameter(command, "@TipoMedicion", (int)item.TipoMedicion);
        AddParameter(command, "@Valor", item.Valor);
        AddParameter(command, "@Unidad", item.Unidad);
        AddParameter(command, "@FechaHora", item.FechaHora);
        AddParameter(command, "@Observaciones", item.Observaciones);
    }
}
