using System.ComponentModel.DataAnnotations;

namespace FlashcardLearning.Models
{
    public class DictionaryEntry
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(200)]
        public string Word { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Meaning { get; set; } = string.Empty;

        public DateTime CachedAt { get; set; } = DateTime.Now;
    }
}
