namespace FlashcardLearning.Services;

public interface IUserService
{
    Task<object?> GetProfileAsync(Guid userId);
    Task<bool> UpdateProfileAsync(Guid userId, string? username, string? avatarUrl);
    Task<bool> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword);
    Task<IEnumerable<object>> GetAllUsersAsync();
    Task<bool> DeleteUserAsync(Guid userId, Guid currentUserId);
}
