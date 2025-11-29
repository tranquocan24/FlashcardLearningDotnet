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
        // Lấy top score của mỗi user trong deck này
        var topUserIds = await _dbSet
            .Where(s => s.DeckId == deckId)
            .GroupBy(s => s.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                MaxScore = g.Max(s => s.Score)
            })
            .OrderByDescending(x => x.MaxScore)
            .Take(topCount)
            .ToListAsync();

        if (!topUserIds.Any())
        {
            return new List<StudySession>();
        }

        // Lấy session tương ứng với score cao nhất, bao gồm User
        var topSessions = new List<StudySession>();

        foreach (var item in topUserIds)
        {
            var session = await _dbSet
                .Include(s => s.User)
                .Where(s => s.DeckId == deckId && s.UserId == item.UserId && s.Score == item.MaxScore)
                .OrderByDescending(s => s.DateStudied)
                .FirstOrDefaultAsync();

            if (session != null)
            {
                topSessions.Add(session);
            }
        }

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
