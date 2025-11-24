using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FlashcardLearning.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [JsonIgnore]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "User";

        public string? AvatarUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Deck>? Decks { get; set; }
        public ICollection<StudySession>? StudySessions { get; set; }
    }
}
