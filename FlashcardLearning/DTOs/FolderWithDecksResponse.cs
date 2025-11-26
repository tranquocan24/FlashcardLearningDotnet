using FlashcardLearning.Models;

namespace FlashcardLearning.DTOs;

public class FolderWithDecksResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<DeckSummary> Decks { get; set; } = new List<DeckSummary>();
}

public class DeckSummary
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPublic { get; set; }
    public DateTime CreatedAt { get; set; }
    public int FlashcardCount { get; set; }
}
