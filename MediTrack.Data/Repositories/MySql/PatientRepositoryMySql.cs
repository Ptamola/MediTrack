using MediTrack.Core.Interfaces.Repositories;
using MediTrack.Core.Models;
using MediTrack.Data.Config;
using MySqlConnector;

namespace MediTrack.Data.Repositories.MySql;

public class PatientRepositoryMySql(DatabaseConnectionFactory connectionFactory)
    : MySqlRepositoryBase<Patient>(connectionFactory), IPatientRepository
{
    protected override string SelectSql => """
        SELECT IdUsuario, FechaNacimiento, Telefono, Direccion, ObservacionesGenerales
        FROM pacientes;
        """;

    protected override string InsertSql => """
        INSERT INTO pacientes (IdUsuario, FechaNacimiento, Telefono, Direccion, ObservacionesGenerales)
        VALUES (@IdUsuario, @FechaNacimiento, @Telefono, @Direccion, @ObservacionesGenerales)
        ON DUPLICATE KEY UPDATE
            FechaNacimiento = VALUES(FechaNacimiento),
            Telefono = VALUES(Telefono),
            Direccion = VALUES(Direccion),
            ObservacionesGenerales = VALUES(ObservacionesGenerales);
        """;

    protected override Patient Map(MySqlDataReader reader) => new()
    {
        IdUsuario = GetGuid(reader, "IdUsuario"),
        FechaNacimiento = GetDateTime(reader, "FechaNacimiento"),
        Telefono = GetString(reader, "Telefono"),
        Direccion = GetString(reader, "Direccion"),
        ObservacionesGenerales = GetString(reader, "ObservacionesGenerales")
    };

    protected override void AddInsertParameters(MySqlCommand command, Patient item)
    {
        AddParameter(command, "@IdUsuario", item.IdUsuario.ToString());
        AddParameter(command, "@FechaNacimiento", item.FechaNacimiento.Date);
        AddParameter(command, "@Telefono", item.Telefono);
        AddParameter(command, "@Direccion", item.Direccion);
        AddParameter(command, "@ObservacionesGenerales", item.ObservacionesGenerales);
    }
}
