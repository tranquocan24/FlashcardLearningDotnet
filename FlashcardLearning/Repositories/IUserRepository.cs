using FlashcardLearning.Models;

namespace FlashcardLearning.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserProfileAsync(Guid userId);
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<bool> ChangePasswordAsync(Guid userId, string newPasswordHash);
}
