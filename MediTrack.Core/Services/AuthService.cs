using MediTrack.Core.DTOs;
using MediTrack.Core.Enums;
using MediTrack.Core.Helpers;
using MediTrack.Core.Interfaces.Repositories;
using MediTrack.Core.Interfaces.Services;
using MediTrack.Core.Models;

namespace MediTrack.Core.Services;

/// <summary>
/// Servicio de autenticacion y registro publico de pacientes.
/// Valida credenciales, verifica hashes y devuelve el usuario con su rol.
/// </summary>
public class AuthService(IUserRepository userRepository, IPatientRepository patientRepository) : IAuthService
{
    /// <summary>
    /// Permite iniciar sesion con nombre de usuario o email, siempre que el usuario este activo.
    /// </summary>
    public async Task<AuthResult> LoginAsync(string usuarioOEmail, string password)
    {
        var users = await userRepository.GetAllAsync();
        var user = users.FirstOrDefault(u =>
            u.Activo &&
            (u.NombreUsuario.Equals(usuarioOEmail, StringComparison.OrdinalIgnoreCase) ||
             u.Email.Equals(usuarioOEmail, StringComparison.OrdinalIgnoreCase)));

        if (user == null || !PasswordHelper.VerifyPassword(password, user.PasswordHash))
        {
            return new AuthResult { EsValido = false, Mensaje = "Usuario o contraseña incorrectos." };
        }

        return new AuthResult
        {
            EsValido = true,
            Mensaje = "Inicio de sesión correcto.",
            Usuario = user
        };
    }

    /// <summary>
    /// Registra pacientes desde la pantalla publica, creando usuario y perfil de paciente asociado.
    /// </summary>
    public async Task<OperationResult> RegisterPatientAsync(RegisterRequest request)
    {
        var validation = ValidationHelper.ValidateRegistration(request);
        if (!validation.Success)
        {
            return validation;
        }

        var users = await userRepository.GetAllAsync();
        if (users.Any(u => u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase)))
        {
            return OperationResult.Fail("Ya existe un usuario con ese correo electrónico.");
        }

        if (users.Any(u => u.NombreUsuario.Equals(request.NombreUsuario, StringComparison.OrdinalIgnoreCase)))
        {
            return OperationResult.Fail("El nombre de usuario ya está en uso.");
        }

        var user = new User
        {
            Nombre = request.Nombre.Trim(),
            Apellidos = request.Apellidos.Trim(),
            Email = request.Email.Trim(),
            NombreUsuario = request.NombreUsuario.Trim(),
            PasswordHash = PasswordHelper.HashPassword(request.Password),
            Rol = UserRole.Paciente,
            Activo = true,
            FechaCreacion = DateTime.Now
        };

        users.Add(user);
        await userRepository.SaveAllAsync(users);

        var patients = await patientRepository.GetAllAsync();
        patients.Add(new Patient
        {
            IdUsuario = user.Id,
            FechaNacimiento = DateTime.Today.AddYears(-request.Edad)
        });

        await patientRepository.SaveAllAsync(patients);
        return OperationResult.Ok("Registro completado correctamente. Ya puedes iniciar sesión.");
    }
}
