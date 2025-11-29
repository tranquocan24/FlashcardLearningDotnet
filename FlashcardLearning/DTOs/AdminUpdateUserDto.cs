using System.ComponentModel.DataAnnotations;

namespace FlashcardLearning.DTOs;

public class AdminUpdateUserDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [EmailAddress]
    public string? NewEmail { get; set; }

    [MinLength(6)]
    public string? NewPassword { get; set; }
}
