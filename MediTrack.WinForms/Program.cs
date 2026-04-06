using MediTrack.WinForms.Config;
using MediTrack.WinForms.Forms.Auth;
using QuestPDF.Infrastructure;

namespace MediTrack.WinForms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        QuestPDF.Settings.License = LicenseType.Community;
        ApplicationConfiguration.Initialize();

        var services = AppBootstrapper.BuildAsync().GetAwaiter().GetResult();
        var session = new AppSession();

        Application.Run(new LoginForm(services, session));
    }
}
