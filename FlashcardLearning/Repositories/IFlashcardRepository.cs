using FlashcardLearning.Models;

namespace FlashcardLearning.Repositories;

public interface IFlashcardRepository : IRepository<Flashcard>
{
    Task<Flashcard?> GetFlashcardWithDeckAsync(Guid flashcardId);
    Task<IEnumerable<Flashcard>> GetFlashcardsByDeckIdAsync(Guid deckId);
    Task<bool> IsUserOwnerOfDeckAsync(Guid flashcardId, Guid userId);
}
