using FlashcardLearning.Models;

namespace FlashcardLearning.DTOs;

public class DeckResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? UserId { get; set; }
    public Guid? FolderId { get; set; }
    public int  FlashcardCount { get; set; }
}

public class DeckDetailResponse : DeckResponse
{
    public List<FlashcardResponse> Flashcards { get; set; } = new();
}
        
public class FlashcardResponse
{
    public Guid Id { get; set; }
    public string Term { get; set; } = string.Empty;
    public string Definition { get; set; } = string.Empty;
    public string? Example { get; set; }
    public string? ImageUrl { get; set; }
    public string? AudioUrl { get; set; }
}
