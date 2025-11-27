using FlashcardLearning.DTOs;

namespace FlashcardLearning.Services;

/// <summary>
/// Service interface for Deck business logic
/// </summary>
public interface IDeckService
{
    // Query operations
    Task<IEnumerable<DeckResponse>> GetDecksForUserAsync(Guid userId, bool isAdmin);
    Task<DeckDetailResponse?> GetDeckByIdAsync(Guid deckId, Guid currentUserId, bool isAdmin);
    
    // Command operations
    Task<DeckResponse> CreateDeckAsync(CreateDeckRequest request, Guid userId);
    Task<bool> UpdateDeckAsync(Guid deckId, UpdateDeckRequest request, Guid userId, bool isAdmin);
    Task<bool> DeleteDeckAsync(Guid deckId, Guid userId, bool isAdmin);
    
    // Validation & Business rules
    Task<bool> CanUserAccessDeckAsync(Guid deckId, Guid userId, bool isAdmin);
    Task<bool> CanUserModifyDeckAsync(Guid deckId, Guid userId, bool isAdmin);
}
