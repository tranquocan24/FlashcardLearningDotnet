using FlashcardLearning.Models;
using Microsoft.EntityFrameworkCore;

namespace FlashcardLearning.Repositories
{
    /// <summary>
    /// Repository implementation cho Dictionary Cache
    /// </summary>
    public class DictionaryRepository : IDictionaryRepository
    {
        private readonly AppDbContext _context;

        public DictionaryRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// L?y t? v?ng theo Word (case-insensitive)
        /// </summary>
        public async Task<DictionaryEntry?> GetByWordAsync(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return null;

            var normalizedWord = word.Trim().ToLower();

            return await _context.DictionaryEntries
                .FirstOrDefaultAsync(d => d.Word == normalizedWord);
        }

        /// <summary>
        /// Thêm t? v?ng m?i vào cache
        /// </summary>
        public async Task AddAsync(DictionaryEntry entry)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            // Normalize word tr??c khi l?u
            entry.Word = entry.Word.Trim().ToLower();
            entry.CachedAt = DateTime.UtcNow;

            _context.DictionaryEntries.Add(entry);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Ki?m tra t? có t?n t?i không
        /// </summary>
        public async Task<bool> ExistsAsync(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return false;

            var normalizedWord = word.Trim().ToLower();

            return await _context.DictionaryEntries
                .AnyAsync(d => d.Word == normalizedWord);
        }
    }
}
