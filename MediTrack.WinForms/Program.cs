using MediTrack.WinForms.Config;
using MediTrack.WinForms.Diagnostics;
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
            var logPath = StartupErrorLogger.Write(ex);
            MessageBox.Show(
                $"No se pudo iniciar MediTrack.\n\nError: {ex.Message}\n\nSe ha guardado un informe en:\n{logPath}",
                "Error de arranque",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
