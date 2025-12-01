using FlashcardLearning.Models;
using Microsoft.EntityFrameworkCore;

namespace FlashcardLearning.Repositories
{
    public class DictionaryRepository : Repository<DictionaryEntry>, IDictionaryRepository
    {
        

        public DictionaryRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<DictionaryEntry?> GetByWordAsync(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return null;

            var normalizedWord = word.Trim().ToLower();

            return await _dbSet
                .FirstOrDefaultAsync(d => d.Word == normalizedWord);
        }
        public async Task AddAsync(DictionaryEntry entry)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            entry.Word = entry.Word.Trim().ToLower();
            entry.CachedAt = DateTime.Now;

            _dbSet.Add(entry);
            await SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return false;

            var normalizedWord = word.Trim().ToLower();

            return await _dbSet
                .AnyAsync(d => d.Word == normalizedWord);
        }
    }
}
