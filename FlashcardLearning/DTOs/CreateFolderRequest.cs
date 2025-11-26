using System.ComponentModel.DataAnnotations;

namespace FlashcardLearning.DTOs;

public class CreateFolderRequest
{
    [Required(ErrorMessage = "Tên th? m?c không ???c ?? tr?ng")]
    [MaxLength(200, ErrorMessage = "Tên th? m?c không ???c v??t quá 200 ký t?")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "Mô t? không ???c v??t quá 1000 ký t?")]
    public string? Description { get; set; }
}
