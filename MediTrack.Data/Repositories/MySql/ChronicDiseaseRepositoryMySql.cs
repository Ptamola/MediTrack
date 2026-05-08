using MediTrack.Core.Interfaces.Repositories;
using MediTrack.Core.Models;
using MediTrack.Data.Config;
using MySqlConnector;

namespace MediTrack.Data.Repositories.MySql;

public class ChronicDiseaseRepositoryMySql(DatabaseConnectionFactory connectionFactory)
    : MySqlRepositoryBase<ChronicDisease>(connectionFactory), IChronicDiseaseRepository
{
    protected override string SelectSql => """
        SELECT Id, Nombre, Descripcion
        FROM enfermedades_cronicas
        ORDER BY Nombre;
        """;

    protected override string InsertSql => """
        INSERT INTO enfermedades_cronicas (Id, Nombre, Descripcion)
        VALUES (@Id, @Nombre, @Descripcion)
        ON DUPLICATE KEY UPDATE
            Nombre = VALUES(Nombre),
            Descripcion = VALUES(Descripcion);
        """;

    protected override ChronicDisease Map(MySqlDataReader reader) => new()
    {
        Id = GetGuid(reader, "Id"),
        Nombre = GetString(reader, "Nombre"),
        Descripcion = GetString(reader, "Descripcion")
    };

    protected override void AddInsertParameters(MySqlCommand command, ChronicDisease item)
    {
        AddParameter(command, "@Id", item.Id.ToString());
        AddParameter(command, "@Nombre", item.Nombre);
        AddParameter(command, "@Descripcion", item.Descripcion);
    }
}
