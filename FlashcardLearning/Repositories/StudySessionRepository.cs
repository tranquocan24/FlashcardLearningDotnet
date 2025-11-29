using FlashcardLearning.Models;
using Microsoft.EntityFrameworkCore;

namespace FlashcardLearning.Repositories;

public class StudySessionRepository : Repository<StudySession>, IStudySessionRepository
{
    public StudySessionRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<StudySession>> GetSessionsByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Include(s => s.Deck)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.DateStudied)
            .ToListAsync();
    }

    public async Task<IEnumerable<StudySession>> GetSessionsByDeckIdAsync(Guid deckId)
    {
        return await _dbSet
            .Include(s => s.User)
            .Where(s => s.DeckId == deckId)
            .OrderByDescending(s => s.DateStudied)
            .ToListAsync();
    }

    public async Task<IEnumerable<StudySession>> GetLeaderboardAsync(Guid deckId, int topCount = 10)
    {
        var topSessions = await _dbSet
        .Where(s => s.DeckId == deckId)
        .GroupBy(s => s.UserId)
        .Select(g => g.OrderByDescending(s => s.Score).First())
        .OrderByDescending(s => s.Score)
        .Take(topCount)
        .ToListAsync();

        return topSessions;
    }

    public async Task<IEnumerable<StudySession>> GetAllSessionsAsync()
    {
        return await _dbSet
            .Include(s => s.Deck)
            .Include(s => s.User)
            .OrderByDescending(s => s.DateStudied)
            .ToListAsync();
    }
}
