namespace MediTrack.Data.Config;

/// <summary>
/// Representa la configuracion de conexion a MySQL leida desde variables de entorno.
/// Evita guardar credenciales reales dentro del codigo fuente.
/// </summary>
public class DatabaseSettings
{
    public const string DefaultDatabaseName = "meditrak_db";

    public string Server { get; init; } = "localhost";
    public uint Port { get; init; } = 3306;
    public string User { get; init; } = "root";
    public string Password { get; init; } = string.Empty;
    public string DatabaseName { get; init; } = DefaultDatabaseName;
    public bool InitializeOnStartup { get; init; }

    /// <summary>
    /// Crea una configuracion usando variables MEDITRACK_DB_* y valores seguros por defecto
    /// para desarrollo local.
    /// </summary>
    public static DatabaseSettings FromEnvironment()
    {
        return new DatabaseSettings
        {
            Server = GetEnvironmentValue("MEDITRACK_DB_SERVER", "localhost"),
            Port = GetEnvironmentPort("MEDITRACK_DB_PORT", 3306),
            User = GetEnvironmentValue("MEDITRACK_DB_USER", "root"),
            Password = Environment.GetEnvironmentVariable("MEDITRACK_DB_PASSWORD") ?? string.Empty,
            DatabaseName = GetEnvironmentValue("MEDITRACK_DB_NAME", DefaultDatabaseName),
            InitializeOnStartup = GetEnvironmentFlag("MEDITRACK_DB_INIT")
        };
    }

    private static string GetEnvironmentValue(string name, string fallback)
    {
        var value = Environment.GetEnvironmentVariable(name);
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }

    private static uint GetEnvironmentPort(string name, uint fallback)
    {
        var value = Environment.GetEnvironmentVariable(name);
        return uint.TryParse(value, out var port) ? port : fallback;
    }

    private static bool GetEnvironmentFlag(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        return value is not null &&
               (value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("1", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("yes", StringComparison.OrdinalIgnoreCase));
    }
}
