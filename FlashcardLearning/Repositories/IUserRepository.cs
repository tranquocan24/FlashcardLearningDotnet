using FlashcardLearning.Models;

namespace FlashcardLearning.Repositories;

/// <summary>
/// User repository interface
/// </summary>
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserProfileAsync(Guid userId);
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<bool> ChangePasswordAsync(Guid userId, string newPasswordHash);
}
