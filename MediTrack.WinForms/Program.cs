using MediTrack.WinForms.Config;
using MediTrack.WinForms.Diagnostics;
using MediTrack.WinForms.Forms.Auth;
using QuestPDF.Infrastructure;

namespace MediTrack.WinForms;

/// <summary>
/// Punto de entrada de la aplicacion WinForms. Configura QuestPDF, construye las dependencias
/// de la aplicacion y abre la pantalla de inicio de sesion.
/// </summary>
internal static class Program
{
    /// <summary>
    /// Inicia MediTrack y centraliza cualquier error de arranque para que el usuario reciba
    /// un mensaje claro y el equipo pueda revisar el log generado.
    /// </summary>
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
