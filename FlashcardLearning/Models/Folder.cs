using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FlashcardLearning.Models;

public class Folder
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public Guid UserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation Properties
    [JsonIgnore]
    [ForeignKey("UserId")]  
    public User? User { get; set; }
    public ICollection<Deck> Decks { get; set; } = new List<Deck>();
}
