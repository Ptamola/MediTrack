using MediTrack.Core.Interfaces.Repositories;
using MediTrack.Core.Models;
using MediTrack.Data.Config;
using MySqlConnector;

namespace MediTrack.Data.Repositories.MySql;

public class MedicationRepositoryMySql(DatabaseConnectionFactory connectionFactory)
    : MySqlRepositoryBase<Medication>(connectionFactory), IMedicationRepository
{
    protected override string SelectSql => """
        SELECT Id, PacienteId, Nombre, Dosis, Frecuencia, Horario, FechaInicio, FechaFin, Observaciones, Activo
        FROM medicaciones
        ORDER BY FechaInicio DESC;
        """;

    protected override string InsertSql => """
        INSERT INTO medicaciones (Id, PacienteId, Nombre, Dosis, Frecuencia, Horario, FechaInicio, FechaFin, Observaciones, Activo)
        VALUES (@Id, @PacienteId, @Nombre, @Dosis, @Frecuencia, @Horario, @FechaInicio, @FechaFin, @Observaciones, @Activo)
        ON DUPLICATE KEY UPDATE
            PacienteId = VALUES(PacienteId),
            Nombre = VALUES(Nombre),
            Dosis = VALUES(Dosis),
            Frecuencia = VALUES(Frecuencia),
            Horario = VALUES(Horario),
            FechaInicio = VALUES(FechaInicio),
            FechaFin = VALUES(FechaFin),
            Observaciones = VALUES(Observaciones),
            Activo = VALUES(Activo);
        """;

    protected override Medication Map(MySqlDataReader reader) => new()
    {
        Id = GetGuid(reader, "Id"),
        PacienteId = GetGuid(reader, "PacienteId"),
        Nombre = GetString(reader, "Nombre"),
        Dosis = GetString(reader, "Dosis"),
        Frecuencia = GetString(reader, "Frecuencia"),
        Horario = GetString(reader, "Horario"),
        FechaInicio = GetDateTime(reader, "FechaInicio"),
        FechaFin = GetNullableDateTime(reader, "FechaFin"),
        Observaciones = GetString(reader, "Observaciones"),
        Activo = GetBoolean(reader, "Activo")
    };

    protected override void AddInsertParameters(MySqlCommand command, Medication item)
    {
        AddParameter(command, "@Id", item.Id.ToString());
        AddParameter(command, "@PacienteId", item.PacienteId.ToString());
        AddParameter(command, "@Nombre", item.Nombre);
        AddParameter(command, "@Dosis", item.Dosis);
        AddParameter(command, "@Frecuencia", item.Frecuencia);
        AddParameter(command, "@Horario", item.Horario);
        AddParameter(command, "@FechaInicio", item.FechaInicio.Date);
        AddParameter(command, "@FechaFin", item.FechaFin?.Date);
        AddParameter(command, "@Observaciones", item.Observaciones);
        AddParameter(command, "@Activo", item.Activo);
    }
}
