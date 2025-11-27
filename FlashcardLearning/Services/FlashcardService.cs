using FlashcardLearning.Models;
using FlashcardLearning.Repositories;
using Microsoft.Extensions.Logging;

namespace FlashcardLearning.Services;

public class FlashcardService : IFlashcardService
{
    private readonly IFlashcardRepository _flashcardRepository;
    private readonly IDeckRepository _deckRepository;
    private readonly IDictionaryService _dictionaryService;
    private readonly ILogger<FlashcardService> _logger;

    public FlashcardService(
        IFlashcardRepository flashcardRepository,
        IDeckRepository deckRepository,
        IDictionaryService dictionaryService,
        ILogger<FlashcardService> logger)
    {
        _flashcardRepository = flashcardRepository;
        _deckRepository = deckRepository;
        _dictionaryService = dictionaryService;
        _logger = logger;
    }

    public async Task<Flashcard?> GetFlashcardAsync(Guid flashcardId, Guid userId, bool isAdmin)
    {
        var flashcard = await _flashcardRepository.GetFlashcardWithDeckAsync(flashcardId);
        
        if (flashcard == null)
        {
            return null;
        }

        // Check access permission
        if (flashcard.Deck != null &&
            !flashcard.Deck.IsPublic &&
            flashcard.Deck.UserId != userId &&
            !isAdmin)
        {
            throw new UnauthorizedAccessException("B?n không có quy?n truy c?p flashcard này.");
        }

        return flashcard;
    }

    public async Task<Flashcard> CreateFlashcardAsync(Flashcard flashcard, Guid userId, bool isAdmin)
    {
        _logger.LogInformation("Creating flashcard with term: {Term}", flashcard.Term);

        // Validate deck exists
        var deck = await _deckRepository.GetByIdAsync(flashcard.DeckId);
        if (deck == null)
        {
            throw new InvalidOperationException("Deck không t?n t?i.");
        }

        // Check permissions
        if (deck.UserId != userId && !isAdmin)
        {
            throw new UnauthorizedAccessException("B?n không có quy?n thêm flashcard vào deck này.");
        }

        // Prepare data
        flashcard.Id = Guid.NewGuid();
        flashcard.Example = flashcard.Example ?? "";
        flashcard.ImageUrl = flashcard.ImageUrl ?? "";

        // Auto-generate audio (with timeout protection)
        if (string.IsNullOrEmpty(flashcard.AudioUrl))
        {
            try
            {
                _logger.LogInformation("Fetching audio for term: {Term}", flashcard.Term);
                
                string? autoAudioUrl = await _dictionaryService.GetAudioUrlAsync(flashcard.Term);
                flashcard.AudioUrl = autoAudioUrl ?? "";
                
                if (!string.IsNullOrEmpty(autoAudioUrl))
                {
                    _logger.LogInformation("Audio found for term: {Term}", flashcard.Term);
                }
                else
                {
                    _logger.LogInformation("No audio found for term: {Term}", flashcard.Term);
                }
            }
            catch (Exception ex)
            {
                // Don't fail the whole request if audio fetch fails
                _logger.LogWarning(ex, "Failed to fetch audio for term: {Term}", flashcard.Term);
                flashcard.AudioUrl = "";
            }
        }

        // Save to database
        await _flashcardRepository.AddAsync(flashcard);

        _logger.LogInformation("Flashcard created successfully with ID: {Id}", flashcard.Id);

        return flashcard;
    }

    public async Task<bool> UpdateFlashcardAsync(Guid flashcardId, Flashcard flashcardUpdate, Guid userId, bool isAdmin)
    {
        if (flashcardId != flashcardUpdate.Id)
        {
            throw new ArgumentException("ID không kh?p.");
        }

        var existingCard = await _flashcardRepository.GetFlashcardWithDeckAsync(flashcardId);
        
        if (existingCard == null)
        {
            return false;
        }

        // Check permissions
        if (existingCard.Deck != null &&
            existingCard.Deck.UserId != userId &&
            !isAdmin)
        {
            throw new UnauthorizedAccessException("B?n không có quy?n ch?nh s?a flashcard này.");
        }

        existingCard.Term = flashcardUpdate.Term;
        existingCard.Definition = flashcardUpdate.Definition;
        existingCard.Example = flashcardUpdate.Example;
        existingCard.ImageUrl = flashcardUpdate.ImageUrl;

        await _flashcardRepository.UpdateAsync(existingCard);

        return true;
    }

    public async Task<bool> DeleteFlashcardAsync(Guid flashcardId, Guid userId, bool isAdmin)
    {
        var flashcard = await _flashcardRepository.GetFlashcardWithDeckAsync(flashcardId);
        
        if (flashcard == null)
        {
            return false;
        }

        // Check permissions
        if (flashcard.Deck != null &&
            flashcard.Deck.UserId != userId &&
            !isAdmin)
        {
            throw new UnauthorizedAccessException("B?n không có quy?n xóa flashcard này.");
        }

        await _flashcardRepository.DeleteAsync(flashcard);

        return true;
    }
}
