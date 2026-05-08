using MySqlConnector;

namespace MediTrack.Data.Config;

public class DatabaseConnectionFactory
{
    private readonly DatabaseSettings _settings;

    public DatabaseConnectionFactory(DatabaseSettings settings)
    {
        _settings = settings;
    }

    public MySqlConnector.MySqlConnection CreateServerConnection()
    {
        return new MySqlConnector.MySqlConnection(BuildConnectionString(includeDatabase: false));
    }

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
