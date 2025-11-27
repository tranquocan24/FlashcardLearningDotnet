using FlashcardLearning.Models;

namespace FlashcardLearning.Repositories;

/// <summary>
/// Flashcard repository interface
/// </summary>
public interface IFlashcardRepository : IRepository<Flashcard>
{
    Task<Flashcard?> GetFlashcardWithDeckAsync(Guid flashcardId);
    Task<IEnumerable<Flashcard>> GetFlashcardsByDeckIdAsync(Guid deckId);
    Task<bool> IsUserOwnerOfDeckAsync(Guid flashcardId, Guid userId);
}
