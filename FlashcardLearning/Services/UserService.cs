using FlashcardLearning.Repositories;

namespace FlashcardLearning.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<object?> GetProfileAsync(Guid userId)
    {
        var user = await _userRepository.GetUserProfileAsync(userId);
        
        if (user == null)
        {
            return null;
        }

        return new
        {
            user.Id,
            user.Username,
            user.Email,
            user.Role,
            user.AvatarUrl,
            user.CreatedAt,
            DeckCount = user.Decks?.Count ?? 0
        };
    }

    public async Task<bool> UpdateProfileAsync(Guid userId, string? username, string? avatarUrl)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        
        if (user == null)
        {
            return false;
        }

        user.AvatarUrl = avatarUrl;
        
        if (!string.IsNullOrEmpty(username))
        {
            user.Username = username;
        }

        await _userRepository.UpdateAsync(user);
        return true;
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        
        if (user == null)
        {
            return false;
        }

        if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.Password))
        {
            throw new InvalidOperationException("M?t kh?u c? không chính xác.");
        }

        string newPasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        return await _userRepository.ChangePasswordAsync(userId, newPasswordHash);
    }

    public async Task<IEnumerable<object>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllUsersAsync();
        
        return users.Select(u => new
        {
            u.Id,
            u.Username,
            u.Email,
            u.Role,
            u.CreatedAt
        });
    }

    public async Task<bool> DeleteUserAsync(Guid userId, Guid currentUserId)
    {
        if (userId == currentUserId)
        {
            throw new InvalidOperationException("B?n không th? t? xóa tài kho?n c?a chính mình.");
        }

        var user = await _userRepository.GetByIdAsync(userId);
        
        if (user == null)
        {
            return false;
        }

        await _userRepository.DeleteAsync(user);
        return true;
    }
}
