namespace FlashcardLearning.DTOs;

public class FolderResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public int DeckCount { get; set; }
}
