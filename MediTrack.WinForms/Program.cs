using MediTrack.WinForms.Config;
using MediTrack.WinForms.Forms.Auth;
using QuestPDF.Infrastructure;

namespace MediTrack.WinForms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        try
        {
            QuestPDF.Settings.License = LicenseType.Community;
            ApplicationConfiguration.Initialize();

            var services = AppBootstrapper.BuildAsync().GetAwaiter().GetResult();
            var session = new AppSession();

            Application.Run(new LoginForm(services, session));
        }
        catch (Exception ex)
        {
            var logPath = WriteStartupErrorLog(ex);
            MessageBox.Show(
                $"No se pudo iniciar MediTrack.\n\nError: {ex.Message}\n\nSe ha guardado un informe en:\n{logPath}",
                "Error de arranque",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private static string WriteStartupErrorLog(Exception ex)
    {
        var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "startup_error.log");
        var content = $"""
            Fecha y hora: {DateTime.Now:yyyy-MM-dd HH:mm:ss}

            Excepcion:
            {ex}

            InnerException:
            {ex.InnerException?.ToString() ?? "Sin InnerException"}

            Variables de entorno no sensibles:
            MEDITRACK_DB_SERVER={Environment.GetEnvironmentVariable("MEDITRACK_DB_SERVER") ?? "(no definido)"}
            MEDITRACK_DB_PORT={Environment.GetEnvironmentVariable("MEDITRACK_DB_PORT") ?? "(no definido)"}
            MEDITRACK_DB_USER={Environment.GetEnvironmentVariable("MEDITRACK_DB_USER") ?? "(no definido)"}
            MEDITRACK_DB_NAME={Environment.GetEnvironmentVariable("MEDITRACK_DB_NAME") ?? "(no definido)"}
            MEDITRACK_DB_INIT={Environment.GetEnvironmentVariable("MEDITRACK_DB_INIT") ?? "(no definido)"}
            """;

        File.WriteAllText(logPath, content);
        return logPath;
    }
}
