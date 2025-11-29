using FlashcardLearning.Models;
using FlashcardLearning.Repositories;

namespace FlashcardLearning.Services;

public class StudySessionService : IStudySessionService
{
    private readonly IStudySessionRepository _studySessionRepository;
    private readonly IDeckRepository _deckRepository;

    public StudySessionService(
        IStudySessionRepository studySessionRepository,
        IDeckRepository deckRepository)
    {
        _studySessionRepository = studySessionRepository;
        _deckRepository = deckRepository;
    }

    public async Task<StudySession> CreateSessionAsync(StudySession session, Guid userId)
    {
        // Validate deck exists
        var deck = await _deckRepository.GetByIdAsync(session.DeckId);
        if (deck == null)
        {
            throw new InvalidOperationException("Deck is not exist.");
        }

        // Set user and session data
        session.UserId = userId;
        session.Id = Guid.NewGuid();
        session.DateStudied = DateTime.Now;

        // Validate score
        if (session.TotalCards > 0 && session.Score > session.TotalCards)
        {
            throw new InvalidOperationException("Score can not be higher than total cards.");
        }

        // Set default mode if not provided
        if (string.IsNullOrEmpty(session.Mode))
        {
            session.Mode = "Flashcard";
        }

        await _studySessionRepository.AddAsync(session);

        return session;
    }

    public async Task<IEnumerable<object>> GetMyHistoryAsync(Guid userId)
    {
        var sessions = await _studySessionRepository.GetSessionsByUserIdAsync(userId);

        return sessions.Select(s => new
        {
            s.Id,
            s.DateStudied,
            s.Score,
            s.TotalCards,
            s.Mode,
            DeckTitle = s.Deck != null ? s.Deck.Title : "Deck was deleted",
            DeckId = s.DeckId
        });
    }

    public async Task<IEnumerable<object>> GetLeaderboardAsync(Guid deckId)
    {
        // Validate deck exists
        var deck = await _deckRepository.GetByIdAsync(deckId);
        if (deck == null)
        {
            throw new InvalidOperationException("Deck is not exist.");
        }

        var leaderboard = await _studySessionRepository.GetLeaderboardAsync(deckId, 10);

        return leaderboard.Select(s => new
        {
            UserName = s.User != null ? s.User.Username : "Unknown",
            Avatar = s.User != null ? s.User.AvatarUrl : null,
            s.Score,
            s.TotalCards,
            Date = s.DateStudied
        });
    }

    public async Task<IEnumerable<StudySession>> GetAllHistoryAsync()
    {
        return await _studySessionRepository.GetAllSessionsAsync();
    }

    public async Task<bool> DeleteSessionAsync(Guid sessionId)
    {
        var session = await _studySessionRepository.GetByIdAsync(sessionId);
        
        if (session == null)
        {
            return false;
        }

        await _studySessionRepository.DeleteAsync(session);
        return true;
    }
}
