using MediTrack.Core.Interfaces.Repositories;
using MediTrack.Core.Models;
using MediTrack.Data.Config;
using MySqlConnector;

namespace MediTrack.Data.Repositories.MySql;

public class DoctorRepositoryMySql(DatabaseConnectionFactory connectionFactory)
    : MySqlRepositoryBase<Doctor>(connectionFactory), IDoctorRepository
{
    protected override string SelectSql => """
        SELECT IdUsuario, NumeroColegiado, Especialidad
        FROM doctores;
        """;

    protected override string InsertSql => """
        INSERT INTO doctores (IdUsuario, NumeroColegiado, Especialidad)
        VALUES (@IdUsuario, @NumeroColegiado, @Especialidad)
        ON DUPLICATE KEY UPDATE
            NumeroColegiado = VALUES(NumeroColegiado),
            Especialidad = VALUES(Especialidad);
        """;

    protected override Doctor Map(MySqlDataReader reader) => new()
    {
        IdUsuario = GetGuid(reader, "IdUsuario"),
        NumeroColegiado = GetString(reader, "NumeroColegiado"),
        Especialidad = GetString(reader, "Especialidad")
    };

    protected override void AddInsertParameters(MySqlCommand command, Doctor item)
    {
        AddParameter(command, "@IdUsuario", item.IdUsuario.ToString());
        AddParameter(command, "@NumeroColegiado", item.NumeroColegiado);
        AddParameter(command, "@Especialidad", item.Especialidad);
    }
}
