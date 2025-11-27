using FlashcardLearning.DTOs;
using FlashcardLearning.Models;
using FlashcardLearning.Repositories;

namespace FlashcardLearning.Services;

/// <summary>
/// Service implementation for Deck business logic
/// Encapsulates all business rules and validations
/// </summary>
public class DeckService : IDeckService
{
    private readonly IDeckRepository _deckRepository;
    private readonly IFolderRepository _folderRepository;

    public DeckService(
        IDeckRepository deckRepository,
        IFolderRepository folderRepository)
    {
        _deckRepository = deckRepository;
        _folderRepository = folderRepository;
    }

    #region Query Operations

    public async Task<IEnumerable<DeckResponse>> GetDecksForUserAsync(Guid userId, bool isAdmin)
    {
        var decks = await _deckRepository.GetDecksAccessibleByUserAsync(userId, isAdmin);
        
        return decks.Select(deck => new DeckResponse
        {
            Id = deck.Id,
            Title = deck.Title,
            Description = deck.Description,
            IsPublic = deck.IsPublic,
            CreatedAt = deck.CreatedAt,
            UserId = deck.UserId,
            FolderId = deck.FolderId,
            FlashcardCount = deck.Flashcards?.Count ?? 0
        });
    }

    public async Task<DeckDetailResponse?> GetDeckByIdAsync(Guid deckId, Guid currentUserId, bool isAdmin)
    {
        var deck = await _deckRepository.GetDeckWithFlashcardsAsync(deckId);
        
        if (deck == null)
            return null;

        // Check access permission
        if (!await CanUserAccessDeckAsync(deckId, currentUserId, isAdmin))
            return null;

        return new DeckDetailResponse
        {
            Id = deck.Id,
            Title = deck.Title,
            Description = deck.Description,
            IsPublic = deck.IsPublic,
            CreatedAt = deck.CreatedAt,
            UserId = deck.UserId,
            FolderId = deck.FolderId,
            FlashcardCount = deck.Flashcards?.Count ?? 0,
            Flashcards = deck.Flashcards?.Select(f => new FlashcardResponse
            {
                Id = f.Id,
                Term = f.Term,
                Definition = f.Definition,
                Example = f.Example,
                ImageUrl = f.ImageUrl,
                AudioUrl = f.AudioUrl
            }).ToList() ?? new List<FlashcardResponse>()
        };
    }

    #endregion

    #region Command Operations

    public async Task<DeckResponse> CreateDeckAsync(CreateDeckRequest request, Guid userId)
    {
        // Validate folder if provided
        if (request.FolderId.HasValue)
        {
            var folderExists = await _folderRepository.ExistsAsync(request.FolderId.Value);
            if (!folderExists)
                throw new InvalidOperationException("Folder does not exist.");

            var isOwner = await _folderRepository.IsUserOwnerAsync(request.FolderId.Value, userId);
            if (!isOwner)
                throw new UnauthorizedAccessException("You do not have permission to add deck to this folder.");
        }

        // Create deck entity
        var deck = new Deck
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description ?? string.Empty,
            IsPublic = request.IsPublic,
            UserId = userId,
            FolderId = request.FolderId,
            CreatedAt = DateTime.UtcNow
        };

        // Save to database
        await _deckRepository.AddAsync(deck);

        // Return DTO
        return new DeckResponse
        {
            Id = deck.Id,
            Title = deck.Title,
            Description = deck.Description,
            IsPublic = deck.IsPublic,
            CreatedAt = deck.CreatedAt,
            UserId = deck.UserId,
            FolderId = deck.FolderId,
            FlashcardCount = 0
        };
    }

    public async Task<bool> UpdateDeckAsync(Guid deckId, UpdateDeckRequest request, Guid userId, bool isAdmin)
    {
        var deck = await _deckRepository.GetByIdAsync(deckId);
        
        if (deck == null)
            return false;

        // Check permission
        if (!await CanUserModifyDeckAsync(deckId, userId, isAdmin))
            throw new UnauthorizedAccessException("You do not have permission to update this deck.");

        // Validate folder if changed
        if (request.FolderId.HasValue)
        {
            var folderExists = await _folderRepository.ExistsAsync(request.FolderId.Value);
            if (!folderExists)
                throw new InvalidOperationException("Folder does not exist.");

            var isOwner = await _folderRepository.IsUserOwnerAsync(request.FolderId.Value, userId);
            if (!isOwner && !isAdmin)
                throw new UnauthorizedAccessException("You do not have permission to move deck to this folder.");
        }

        // Update properties
        deck.Title = request.Title;
        deck.Description = request.Description ?? string.Empty;
        deck.IsPublic = request.IsPublic;
        deck.FolderId = request.FolderId;

        // Save changes
        await _deckRepository.UpdateAsync(deck);

        return true;
    }

    public async Task<bool> DeleteDeckAsync(Guid deckId, Guid userId, bool isAdmin)
    {
        var deck = await _deckRepository.GetByIdAsync(deckId);
        
        if (deck == null)
            return false;

        // Check permission
        if (!await CanUserModifyDeckAsync(deckId, userId, isAdmin))
            throw new UnauthorizedAccessException("You do not have permission to delete this deck.");

        // Delete deck
        await _deckRepository.DeleteAsync(deck);

        return true;
    }

    #endregion

    #region Validation & Business Rules

    public async Task<bool> CanUserAccessDeckAsync(Guid deckId, Guid userId, bool isAdmin)
    {
        return await _deckRepository.CanUserAccessAsync(deckId, userId, isAdmin);
    }

    public async Task<bool> CanUserModifyDeckAsync(Guid deckId, Guid userId, bool isAdmin)
    {
        if (isAdmin)
            return true;

        return await _deckRepository.IsUserOwnerAsync(deckId, userId);
    }

    #endregion
}
