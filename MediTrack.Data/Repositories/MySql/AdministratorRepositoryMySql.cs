using MediTrack.Core.Interfaces.Repositories;
using MediTrack.Core.Models;
using MediTrack.Data.Config;
using MySqlConnector;

namespace MediTrack.Data.Repositories.MySql;

public class AdministratorRepositoryMySql(DatabaseConnectionFactory connectionFactory)
    : MySqlRepositoryBase<Administrator>(connectionFactory), IAdministratorRepository
{
    protected override string SelectSql => """
        SELECT IdUsuario
        FROM administradores;
        """;

    protected override string InsertSql => """
        INSERT INTO administradores (IdUsuario)
        VALUES (@IdUsuario)
        ON DUPLICATE KEY UPDATE
            IdUsuario = VALUES(IdUsuario);
        """;

    protected override Administrator Map(MySqlDataReader reader) => new()
    {
        IdUsuario = GetGuid(reader, "IdUsuario")
    };

    protected override void AddInsertParameters(MySqlCommand command, Administrator item)
    {
        AddParameter(command, "@IdUsuario", item.IdUsuario.ToString());
    }
}
