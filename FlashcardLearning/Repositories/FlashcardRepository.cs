using FlashcardLearning.Models;
using Microsoft.EntityFrameworkCore;

namespace FlashcardLearning.Repositories;

/// <summary>
/// Flashcard repository implementation
/// </summary>
public class FlashcardRepository : Repository<Flashcard>, IFlashcardRepository
{
    public FlashcardRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Flashcard?> GetFlashcardWithDeckAsync(Guid flashcardId)
    {
        return await _dbSet
            .Include(f => f.Deck)
            .FirstOrDefaultAsync(f => f.Id == flashcardId);
    }

    public async Task<IEnumerable<Flashcard>> GetFlashcardsByDeckIdAsync(Guid deckId)
    {
        return await _dbSet
            .Where(f => f.DeckId == deckId)
            .ToListAsync();
    }

    public async Task<bool> IsUserOwnerOfDeckAsync(Guid flashcardId, Guid userId)
    {
        return await _dbSet
            .AnyAsync(f => f.Id == flashcardId && f.Deck != null && f.Deck.UserId == userId);
    }
}
