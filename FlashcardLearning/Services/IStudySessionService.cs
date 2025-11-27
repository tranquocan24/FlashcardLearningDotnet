using FlashcardLearning.Models;

namespace FlashcardLearning.Services;

public interface IStudySessionService
{
    Task<StudySession> CreateSessionAsync(StudySession session, Guid userId);
    Task<IEnumerable<object>> GetMyHistoryAsync(Guid userId);
    Task<IEnumerable<object>> GetLeaderboardAsync(Guid deckId);
    Task<IEnumerable<StudySession>> GetAllHistoryAsync();
    Task<bool> DeleteSessionAsync(Guid sessionId);
}
