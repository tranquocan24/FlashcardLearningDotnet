using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FlashcardLearning.Models
{
    public class Flashcard
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Term { get; set; } = string.Empty;

        [Required]
        public string Definition {  get; set; } = string.Empty;

        public string? Example { get; set; }

        public string? ImageUrl { get; set; }

        public Guid DeckId { get; set; }

        [JsonIgnore]
        [ForeignKey("DeskId")]
        public Deck? Deck { get; set; }
    }
}
