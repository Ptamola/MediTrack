using MediTrack.Core.DTOs;
using MediTrack.Core.Enums;
using MediTrack.Core.Helpers;
using MediTrack.Core.Interfaces.Repositories;
using MediTrack.Core.Interfaces.Services;
using MediTrack.Core.Models;

namespace MediTrack.Core.Services;

/// <summary>
/// Servicio de administracion de usuarios. Centraliza altas, ediciones, roles y activacion.
/// </summary>
public class UserService(
    IUserRepository userRepository,
    IPatientRepository patientRepository,
    IDoctorRepository doctorRepository,
    IAdministratorRepository administratorRepository) : IUserService
{
    public Task<List<User>> GetAllAsync() => userRepository.GetAllAsync();

    public async Task<User?> GetByIdAsync(Guid userId) =>
        (await userRepository.GetAllAsync()).FirstOrDefault(u => u.Id == userId);

    public async Task<List<User>> GetByRoleAsync(UserRole role) =>
        (await userRepository.GetAllAsync()).Where(u => u.Rol == role).OrderBy(u => u.Apellidos).ThenBy(u => u.Nombre).ToList();

    /// <summary>
    /// Crea un usuario y su perfil minimo segun el rol seleccionado.
    /// </summary>
    public async Task<OperationResult> CreateAsync(User user, string plainPassword)
    {
        if (string.IsNullOrWhiteSpace(user.Nombre) ||
            string.IsNullOrWhiteSpace(user.Apellidos) ||
            string.IsNullOrWhiteSpace(user.Email) ||
            string.IsNullOrWhiteSpace(user.NombreUsuario))
        {
            return OperationResult.Fail("Nombre, apellidos, email y nombre de usuario son obligatorios.");
        }

        if (!ValidationHelper.IsValidEmail(user.Email))
        {
            return OperationResult.Fail("El email no es válido.");
        }

        if (!ValidationHelper.IsStrongPassword(plainPassword))
        {
            return OperationResult.Fail("La contraseña no cumple las reglas de seguridad.");
        }

        var users = await userRepository.GetAllAsync();
        if (users.Any(u => u.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase)))
        {
            return OperationResult.Fail("Ya existe un usuario con ese correo.");
        }

        if (users.Any(u => u.NombreUsuario.Equals(user.NombreUsuario, StringComparison.OrdinalIgnoreCase)))
        {
            return OperationResult.Fail("Ya existe un usuario con ese nombre de usuario.");
        }

        user.Id = Guid.NewGuid();
        user.PasswordHash = PasswordHelper.HashPassword(plainPassword);
        user.FechaCreacion = DateTime.Now;
        users.Add(user);
        await userRepository.SaveAllAsync(users);

        await EnsureProfileAsync(user);
        return OperationResult.Ok("Usuario creado correctamente.");
    }

    /// <summary>
    /// Actualiza datos basicos sin modificar la contrasena.
    /// </summary>
    public async Task<OperationResult> UpdateAsync(User user)
    {
        var users = await userRepository.GetAllAsync();
        var existing = users.FirstOrDefault(u => u.Id == user.Id);
        if (existing == null)
        {
            return OperationResult.Fail("No se encontró el usuario seleccionado.");
        }

        if (string.IsNullOrWhiteSpace(user.Nombre) ||
            string.IsNullOrWhiteSpace(user.Apellidos) ||
            string.IsNullOrWhiteSpace(user.Email) ||
            string.IsNullOrWhiteSpace(user.NombreUsuario))
        {
            return OperationResult.Fail("Nombre, apellidos, email y nombre de usuario son obligatorios.");
        }

        if (!ValidationHelper.IsValidEmail(user.Email))
        {
            return OperationResult.Fail("El email no es válido.");
        }

        if (user.NombreUsuario.Trim().Length < Constants.AppConstants.MinimumUsernameLength)
        {
            return OperationResult.Fail($"El nombre de usuario debe tener al menos {Constants.AppConstants.MinimumUsernameLength} caracteres.");
        }

        if (users.Any(u => u.Id != user.Id && u.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase)))
        {
            return OperationResult.Fail("Ya existe otro usuario con ese correo.");
        }

        if (users.Any(u => u.Id != user.Id && u.NombreUsuario.Equals(user.NombreUsuario, StringComparison.OrdinalIgnoreCase)))
        {
            return OperationResult.Fail("Ya existe otro usuario con ese nombre de usuario.");
        }

        existing.Nombre = user.Nombre.Trim();
        existing.Apellidos = user.Apellidos.Trim();
        existing.Email = user.Email.Trim();
        existing.NombreUsuario = user.NombreUsuario.Trim();
        existing.Rol = user.Rol;
        existing.Activo = user.Activo;

        await userRepository.SaveAllAsync(users);
        await EnsureProfileAsync(existing);
        return OperationResult.Ok("Usuario actualizado.");
    }

    /// <summary>
    /// Activa o desactiva usuarios evitando dejar el sistema sin administradores activos.
    /// </summary>
    public async Task<OperationResult> ToggleActiveAsync(Guid userId, bool active)
    {
        var users = await userRepository.GetAllAsync();
        var user = users.FirstOrDefault(u => u.Id == userId);
        if (user == null)
        {
            return OperationResult.Fail("Usuario no encontrado.");
        }

        if (!active && user.Rol == UserRole.Administrador && users.Count(u => u.Rol == UserRole.Administrador && u.Activo) <= 1)
        {
            return OperationResult.Fail("Debe existir al menos un administrador activo en el sistema.");
        }

        user.Activo = active;
        await userRepository.SaveAllAsync(users);
        return OperationResult.Ok(active ? "Usuario activado." : "Usuario desactivado.");
    }

    /// <summary>
    /// Garantiza que cada usuario tenga la entidad de perfil correspondiente a su rol.
    /// </summary>
    private async Task EnsureProfileAsync(User user)
    {
        switch (user.Rol)
        {
            case UserRole.Paciente:
                var patients = await patientRepository.GetAllAsync();
                if (!patients.Any(p => p.IdUsuario == user.Id))
                {
                    patients.Add(new Patient { IdUsuario = user.Id });
                    await patientRepository.SaveAllAsync(patients);
                }
                break;
            case UserRole.Doctor:
                var doctors = await doctorRepository.GetAllAsync();
                if (!doctors.Any(d => d.IdUsuario == user.Id))
                {
                    doctors.Add(new Doctor
                    {
                        IdUsuario = user.Id,
                        Especialidad = "Medicina General",
                        NumeroColegiado = $"COL-{Random.Shared.Next(1000, 9999)}"
                    });
                    await doctorRepository.SaveAllAsync(doctors);
                }
                break;
            case UserRole.Administrador:
                var administrators = await administratorRepository.GetAllAsync();
                if (!administrators.Any(a => a.IdUsuario == user.Id))
                {
                    administrators.Add(new Administrator { IdUsuario = user.Id });
                    await administratorRepository.SaveAllAsync(administrators);
                }
                break;
        }
    }
}
