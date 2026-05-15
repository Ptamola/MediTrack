using MediTrack.Core.Enums;
using MediTrack.Core.Interfaces.Repositories;
using MediTrack.Core.Models;
using MediTrack.Data.Config;
using MySqlConnector;

namespace MediTrack.Data.Repositories.MySql;

/// <summary>
/// Repositorio MySQL de usuarios. Gestiona credenciales hash, rol y estado activo.
/// </summary>
public class UserRepositoryMySql(DatabaseConnectionFactory connectionFactory)
    : MySqlRepositoryBase<User>(connectionFactory), IUserRepository
{
    protected override string SelectSql => """
        SELECT Id, Nombre, Apellidos, Email, NombreUsuario, PasswordHash, Rol, Activo, FechaCreacion
        FROM usuarios
        ORDER BY Apellidos, Nombre;
        """;

    // UPSERT: actualiza el usuario si ya existe por clave primaria o indice unico.
    protected override string InsertSql => """
        INSERT INTO usuarios (Id, Nombre, Apellidos, Email, NombreUsuario, PasswordHash, Rol, Activo, FechaCreacion)
        VALUES (@Id, @Nombre, @Apellidos, @Email, @NombreUsuario, @PasswordHash, @Rol, @Activo, @FechaCreacion)
        ON DUPLICATE KEY UPDATE
            Nombre = VALUES(Nombre),
            Apellidos = VALUES(Apellidos),
            Email = VALUES(Email),
            NombreUsuario = VALUES(NombreUsuario),
            PasswordHash = VALUES(PasswordHash),
            Rol = VALUES(Rol),
            Activo = VALUES(Activo),
            FechaCreacion = VALUES(FechaCreacion);
        """;

    protected override User Map(MySqlDataReader reader) => new()
    {
        Id = GetGuid(reader, "Id"),
        Nombre = GetString(reader, "Nombre"),
        Apellidos = GetString(reader, "Apellidos"),
        Email = GetString(reader, "Email"),
        NombreUsuario = GetString(reader, "NombreUsuario"),
        PasswordHash = GetString(reader, "PasswordHash"),
        Rol = (UserRole)GetInt32(reader, "Rol"),
        Activo = GetBoolean(reader, "Activo"),
        FechaCreacion = GetDateTime(reader, "FechaCreacion")
    };

    protected override void AddInsertParameters(MySqlCommand command, User item)
    {
        AddParameter(command, "@Id", item.Id.ToString());
        AddParameter(command, "@Nombre", item.Nombre);
        AddParameter(command, "@Apellidos", item.Apellidos);
        AddParameter(command, "@Email", item.Email);
        AddParameter(command, "@NombreUsuario", item.NombreUsuario);
        AddParameter(command, "@PasswordHash", item.PasswordHash);
        AddParameter(command, "@Rol", (int)item.Rol);
        AddParameter(command, "@Activo", item.Activo);
        AddParameter(command, "@FechaCreacion", item.FechaCreacion);
    }
}
