using System.ComponentModel.DataAnnotations;

namespace FlashcardLearning.DTOs
{
    public class CreateFolderRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class UpdateFolderRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}
