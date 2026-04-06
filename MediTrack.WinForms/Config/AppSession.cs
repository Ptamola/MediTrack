using MediTrack.Core.Models;

namespace MediTrack.WinForms.Config;

public class AppSession
{
    public User? CurrentUser { get; private set; }
    public Guid? SelectedPatientId { get; set; }

    public bool IsAuthenticated => CurrentUser != null;

    public void SignIn(User user)
    {
        CurrentUser = user;
        SelectedPatientId = user.Rol == Core.Enums.UserRole.Paciente ? user.Id : null;
    }

    public void SignOut()
    {
        CurrentUser = null;
        SelectedPatientId = null;
    }
}
