using System.ComponentModel.DataAnnotations;

namespace FlashcardLearning.DTOs
{
    public class UpdateFolderRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}
