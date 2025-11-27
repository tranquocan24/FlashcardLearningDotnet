using FlashcardLearning.DTOs;

namespace FlashcardLearning.Services;

public interface IAuthService
{
    Task<object> RegisterAsync(RegisterDto request);
    Task<object> LoginAsync(LoginDto request);
}
