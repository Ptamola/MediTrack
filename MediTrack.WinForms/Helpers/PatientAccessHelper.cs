using MediTrack.Core.Enums;
using MediTrack.Core.Models;
using MediTrack.WinForms.Config;

namespace MediTrack.WinForms.Helpers;

public static class PatientAccessHelper
{
    public static async Task<List<User>> GetAccessiblePatientsAsync(ApplicationServices services, AppSession session)
    {
        if (session.CurrentUser == null)
        {
            return [];
        }

        return session.CurrentUser.Rol switch
        {
            UserRole.Paciente => (await services.UserService.GetAllAsync()).Where(u => u.Id == session.CurrentUser.Id).ToList(),
            UserRole.Doctor => await services.DoctorAssignmentService.GetPatientsForDoctorAsync(session.CurrentUser.Id),
            UserRole.Administrador => await services.UserService.GetByRoleAsync(UserRole.Paciente),
            _ => []
        };
    }

    public static async Task<Guid?> EnsureSelectedPatientAsync(ApplicationServices services, AppSession session)
    {
        var patients = await GetAccessiblePatientsAsync(services, session);
        if (patients.Count == 0)
        {
            session.SelectedPatientId = null;
            return null;
        }

        if (session.SelectedPatientId.HasValue && patients.Any(p => p.Id == session.SelectedPatientId.Value))
        {
            return session.SelectedPatientId;
        }

        session.SelectedPatientId = patients[0].Id;
        return session.SelectedPatientId;
    }

    public static void ApplySelectedPatient(ComboBox comboBox, AppSession session)
    {
        if (session.SelectedPatientId.HasValue)
        {
            comboBox.SelectedValue = session.SelectedPatientId.Value;
        }
    }
}
