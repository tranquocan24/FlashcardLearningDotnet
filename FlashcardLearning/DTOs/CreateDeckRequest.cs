using System.ComponentModel.DataAnnotations;

namespace FlashcardLearning.DTOs;

public class CreateDeckRequest
{
    [Required(ErrorMessage = "Title can not be null")]
    [MaxLength(200, ErrorMessage = "Title can not be more than 200 characters")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "Description can not be more than 1000 characters")]
    public string? Description { get; set; }

    public bool IsPublic { get; set; } = false;

    public Guid? FolderId { get; set; }
}
