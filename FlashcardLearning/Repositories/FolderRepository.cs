using FlashcardLearning.Models;
using Microsoft.EntityFrameworkCore;

namespace FlashcardLearning.Repositories;

public class FolderRepository : Repository<Folder>, IFolderRepository
{
    public FolderRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Folder?> GetFolderWithDecksAsync(Guid folderId)
    {
        return await _dbSet
            .Include(f => f.Decks)
            .FirstOrDefaultAsync(f => f.Id == folderId);
    }

    public async Task<Folder?> GetFolderWithDecksAndFlashcardsAsync(Guid folderId)
    {
        return await _dbSet
            .Include(f => f.Decks)
                .ThenInclude(d => d.Flashcards)
            .FirstOrDefaultAsync(f => f.Id == folderId);
    }

    public async Task<IEnumerable<Folder>> GetFoldersByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Folder>> GetFoldersWithDecksAsync(Guid userId)
    {
        return await _dbSet
            .Where(f => f.UserId == userId)
            .Include(f => f.Decks)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }
    
    public async Task<Folder?> GetFolderByNameAsync(string name, Guid userId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(f => f.Name == name && f.UserId == userId);
    }

    public async Task<bool> IsUserOwnerAsync(Guid folderId, Guid userId)
    {
        return await _dbSet.AnyAsync(f => f.Id == folderId && f.UserId == userId);
    }
}
