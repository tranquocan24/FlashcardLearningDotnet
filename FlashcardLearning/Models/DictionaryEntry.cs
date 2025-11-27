using System.ComponentModel.DataAnnotations;

namespace FlashcardLearning.Models
{
    /// <summary>
    /// Entity l?u tr? cache t? v?ng ?ã d?ch (En -> Vi)
    /// </summary>
    public class DictionaryEntry
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// T? ti?ng Anh (ch? th??ng)
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Word { get; set; } = string.Empty;

        /// <summary>
        /// Ngh?a ti?ng Vi?t
        /// </summary>
        [Required]
        [MaxLength(1000)]
        public string Meaning { get; set; } = string.Empty;

        /// <summary>
        /// Th?i gian cache
        /// </summary>
        public DateTime CachedAt { get; set; } = DateTime.UtcNow;
    }
}
