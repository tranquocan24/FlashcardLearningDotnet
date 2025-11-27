using FlashcardLearning.Models;

namespace FlashcardLearning.Repositories
{
    /// <summary>
    /// Interface cho Dictionary Repository
    /// </summary>
    public interface IDictionaryRepository
    {
        /// <summary>
        /// L?y t? v?ng theo Word
        /// </summary>
        Task<DictionaryEntry?> GetByWordAsync(string word);

        /// <summary>
        /// Thêm t? v?ng m?i vào cache
        /// </summary>
        Task AddAsync(DictionaryEntry entry);

        /// <summary>
        /// Ki?m tra t? có t?n t?i không
        /// </summary>
        Task<bool> ExistsAsync(string word);
    }
}
