using MySqlConnector;

namespace MediTrack.Data.Config;

/// <summary>
/// Fabrica conexiones MySQL con MySqlConnector a partir de la configuracion de entorno.
/// Se usa tanto para crear la base como para operar dentro de ella.
/// </summary>
public class DatabaseConnectionFactory
{
    private readonly DatabaseSettings _settings;

    public DatabaseConnectionFactory(DatabaseSettings settings)
    {
        _settings = settings;
    }

    /// <summary>
    /// Crea una conexion al servidor sin seleccionar base de datos; necesaria para CREATE DATABASE.
    /// </summary>
    public MySqlConnector.MySqlConnection CreateServerConnection()
    {
        return new MySqlConnector.MySqlConnection(BuildConnectionString(includeDatabase: false));
    }

    /// <summary>
    /// Crea una conexion apuntando a la base de datos principal de MediTrack.
    /// </summary>
    public MySqlConnector.MySqlConnection CreateDatabaseConnection()
    {
        return new MySqlConnector.MySqlConnection(BuildConnectionString(includeDatabase: true));
    }

    private string BuildConnectionString(bool includeDatabase)
    {
        var builder = new MySqlConnectionStringBuilder
        {
            Server = _settings.Server,
            Port = _settings.Port,
            UserID = _settings.User,
            Password = _settings.Password,
            CharacterSet = "utf8mb4",
            SslMode = MySqlSslMode.Preferred
        };

        if (includeDatabase)
        {
            builder.Database = _settings.DatabaseName;
        }

        return builder.ConnectionString;
    }
}
