using FlashcardLearning.Models;
using Microsoft.EntityFrameworkCore;

namespace FlashcardLearning.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetUserProfileAsync(Guid userId)
    {
        return await _dbSet
            .Include(u => u.Decks)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _dbSet
            .OrderBy(u => u.Username)
            .ToListAsync();
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, string newPasswordHash)
    {
        var user = await _dbSet.FindAsync(userId);
        if (user == null)
        {
            return false;
        }

        user.Password = newPasswordHash;
        await _context.SaveChangesAsync();
        return true;
    }
}
