using FlashcardLearning.Models;

namespace FlashcardLearning.Repositories;

/// <summary>
/// Deck repository interface with domain-specific operations
/// </summary>
public interface IDeckRepository : IRepository<Deck>
{
    // Specialized query methods
    Task<Deck?> GetDeckWithFlashcardsAsync(Guid deckId);
    Task<Deck?> GetDeckWithDetailsAsync(Guid deckId); // Include Flashcards, Owner, Folder
    Task<IEnumerable<Deck>> GetDecksByUserIdAsync(Guid userId);
    Task<IEnumerable<Deck>> GetPublicDecksAsync();
    Task<IEnumerable<Deck>> GetDecksByFolderIdAsync(Guid folderId);
    Task<IEnumerable<Deck>> GetDecksAccessibleByUserAsync(Guid userId, bool isAdmin);
    Task<IEnumerable<Deck>> GetUnassignedDecksAsync(Guid userId);
    
    // Business queries
    Task<bool> IsUserOwnerAsync(Guid deckId, Guid userId);
    Task<bool> CanUserAccessAsync(Guid deckId, Guid userId, bool isAdmin);
}
