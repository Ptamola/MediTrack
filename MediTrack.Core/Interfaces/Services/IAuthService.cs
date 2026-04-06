using MediTrack.Core.DTOs;

namespace MediTrack.Core.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(string usuarioOEmail, string password);
    Task<OperationResult> RegisterPatientAsync(RegisterRequest request);
}
