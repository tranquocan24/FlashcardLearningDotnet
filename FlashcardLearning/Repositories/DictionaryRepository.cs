using FlashcardLearning.Models;
using Microsoft.EntityFrameworkCore;

namespace FlashcardLearning.Repositories
{
    public class DictionaryRepository : IDictionaryRepository
    {
        private readonly AppDbContext _context;

        public DictionaryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DictionaryEntry?> GetByWordAsync(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return null;

            var normalizedWord = word.Trim().ToLower();

            return await _context.DictionaryEntries
                .FirstOrDefaultAsync(d => d.Word == normalizedWord);
        }
        public async Task AddAsync(DictionaryEntry entry)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            entry.Word = entry.Word.Trim().ToLower();
            entry.CachedAt = DateTime.Now;

            _context.DictionaryEntries.Add(entry);
            await _context.SaveChangesAsync();
        }

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
