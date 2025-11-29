using System.ComponentModel.DataAnnotations;

namespace FlashcardLearning.DTOs;

public class CreateFolderRequest
{
    [Required(ErrorMessage = "Name must not be empty")]
    [MaxLength(200, ErrorMessage = "Name must not be more than 200 characters")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "Description can not be more than 1000 characters")]
    public string? Description { get; set; }
}
