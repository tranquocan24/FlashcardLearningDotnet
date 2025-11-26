using System.ComponentModel.DataAnnotations;

namespace FlashcardLearning.DTOs;

public class CreateDeckRequest
{
    [Required(ErrorMessage = "Tiêu ?? không ???c ?? tr?ng")]
    [MaxLength(200, ErrorMessage = "Tiêu ?? không ???c v??t quá 200 ký t?")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "Mô t? không ???c v??t quá 1000 ký t?")]
    public string? Description { get; set; }

    public bool IsPublic { get; set; } = false;

    public Guid? FolderId { get; set; }
}
