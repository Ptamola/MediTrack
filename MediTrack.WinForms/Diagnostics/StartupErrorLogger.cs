namespace MediTrack.WinForms.Diagnostics;

internal static class StartupErrorLogger
{
    private const string LogFileName = "startup_error.log";

    public static string Write(Exception exception)
    {
        var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LogFileName);
        File.WriteAllText(logPath, BuildContent(exception));
        return logPath;
    }

    private static string BuildContent(Exception exception)
    {
        return $"""
            Fecha y hora: {DateTime.Now:yyyy-MM-dd HH:mm:ss}

            Excepcion:
            {exception}

            InnerException:
            {exception.InnerException?.ToString() ?? "Sin InnerException"}

            Variables de entorno no sensibles:
            MEDITRACK_DB_SERVER={ReadEnvironmentValue("MEDITRACK_DB_SERVER")}
            MEDITRACK_DB_PORT={ReadEnvironmentValue("MEDITRACK_DB_PORT")}
            MEDITRACK_DB_USER={ReadEnvironmentValue("MEDITRACK_DB_USER")}
            MEDITRACK_DB_NAME={ReadEnvironmentValue("MEDITRACK_DB_NAME")}
            MEDITRACK_DB_INIT={ReadEnvironmentValue("MEDITRACK_DB_INIT")}
            """;
    }

    private static string ReadEnvironmentValue(string name)
    {
        return Environment.GetEnvironmentVariable(name) ?? "(no definido)";
    }
}
