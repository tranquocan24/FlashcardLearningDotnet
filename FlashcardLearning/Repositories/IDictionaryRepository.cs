using FlashcardLearning.Models;

namespace FlashcardLearning.Repositories
{
    public interface IDictionaryRepository
    {
        Task<DictionaryEntry?> GetByWordAsync(string word);
        Task AddAsync(DictionaryEntry entry);
        Task<bool> ExistsAsync(string word);
    }
}
