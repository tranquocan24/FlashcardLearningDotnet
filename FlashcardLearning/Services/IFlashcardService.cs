using FlashcardLearning.Models;

namespace FlashcardLearning.Services;

public interface IFlashcardService
{
    Task<Flashcard?> GetFlashcardAsync(Guid flashcardId, Guid userId, bool isAdmin);
    Task<Flashcard> CreateFlashcardAsync(Flashcard flashcard, Guid userId, bool isAdmin);
    Task<bool> UpdateFlashcardAsync(Guid flashcardId, Flashcard flashcardUpdate, Guid userId, bool isAdmin);
    Task<bool> DeleteFlashcardAsync(Guid flashcardId, Guid userId, bool isAdmin);
}
