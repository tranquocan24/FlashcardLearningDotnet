using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlashcardLearning.Models
{
    public class StudySession
    {
        [Key]
        public Guid Id { get; set; }

        public DateTime DateStudied { get; set; } = DateTime.Now;

        public int Score { get; set; }

        public int TotalCards { get; set; }

        public string Mode { get; set; } = "Flashcard"; // Study mode: Flashcard / Quiz / Match

        // FK: User
        public Guid UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }

        // FK: Desk
        public Guid DeckId { get; set; }
        [ForeignKey("DeckId")]
        public Deck? Deck { get; set; }
    }
}
