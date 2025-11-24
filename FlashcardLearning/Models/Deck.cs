using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FlashcardLearning.Models
{
    public class Deck
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; }
        public bool IsPublic { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Guid? UserId { get; set; }

        [JsonIgnore]
        [ForeignKey("UserId")]
        public User? Owner { get; set; }

        public ICollection<Flashcard>? Flashcards { get; set; }
    }
}
