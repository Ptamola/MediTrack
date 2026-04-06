using MediTrack.Core.DTOs;
using MediTrack.Core.Enums;
using MediTrack.Core.Models;

namespace MediTrack.Core.Interfaces.Services;

public interface IUserService
{
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(Guid userId);
    Task<List<User>> GetByRoleAsync(UserRole role);
    Task<OperationResult> CreateAsync(User user, string plainPassword);
    Task<OperationResult> UpdateAsync(User user);
    Task<OperationResult> ToggleActiveAsync(Guid userId, bool active);
}
