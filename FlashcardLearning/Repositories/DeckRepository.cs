using FlashcardLearning.Models;
using Microsoft.EntityFrameworkCore;

namespace FlashcardLearning.Repositories;

/// <summary>
/// Deck repository implementation with domain-specific queries
/// </summary>
public class DeckRepository : Repository<Deck>, IDeckRepository
{
    public DeckRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Deck?> GetDeckWithFlashcardsAsync(Guid deckId)
    {
        return await _dbSet
            .Include(d => d.Flashcards)
            .FirstOrDefaultAsync(d => d.Id == deckId);
    }

    public async Task<Deck?> GetDeckWithDetailsAsync(Guid deckId)
    {
        return await _dbSet
            .Include(d => d.Flashcards)
            .Include(d => d.Owner)
            .Include(d => d.Folder)
            .FirstOrDefaultAsync(d => d.Id == deckId);
    }

    public async Task<IEnumerable<Deck>> GetDecksByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Include(d => d.Flashcards)
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Deck>> GetPublicDecksAsync()
    {
        return await _dbSet
            .Include(d => d.Flashcards)
            .Where(d => d.IsPublic)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Deck>> GetDecksByFolderIdAsync(Guid folderId)
    {
        return await _dbSet
            .Include(d => d.Flashcards)
            .Where(d => d.FolderId == folderId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Deck>> GetDecksAccessibleByUserAsync(Guid userId, bool isAdmin)
    {
        if (isAdmin)
        {
            // Admin can see all decks
            return await _dbSet
                .Include(d => d.Flashcards)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        // Regular user: own decks + public decks
        return await _dbSet
            .Include(d => d.Flashcards)
            .Where(d => d.UserId == userId || d.IsPublic)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> IsUserOwnerAsync(Guid deckId, Guid userId)
    {
        return await _dbSet.AnyAsync(d => d.Id == deckId && d.UserId == userId);
    }

    public async Task<bool> CanUserAccessAsync(Guid deckId, Guid userId, bool isAdmin)
    {
        if (isAdmin)
            return true;

        return await _dbSet.AnyAsync(d => 
            d.Id == deckId && 
            (d.UserId == userId || d.IsPublic));
    }

    public async Task<IEnumerable<Deck>> GetUnassignedDecksAsync(Guid userId)
    {
        return await _dbSet
            .Include(d => d.Flashcards)
            .Where(d => d.UserId == userId && d.FolderId == null)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }
}
