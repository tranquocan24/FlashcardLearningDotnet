using FlashcardLearning.Models;

namespace FlashcardLearning.Repositories;

public interface IStudySessionRepository : IRepository<StudySession>
{
    Task<IEnumerable<StudySession>> GetSessionsByUserIdAsync(Guid userId);
    Task<IEnumerable<StudySession>> GetSessionsByDeckIdAsync(Guid deckId);
    Task<IEnumerable<StudySession>> GetLeaderboardAsync(Guid deckId, int topCount = 10);
    Task<IEnumerable<StudySession>> GetAllSessionsAsync();
}
