using FlashcardLearning.DTOs;
using FlashcardLearning.Models;
using FlashcardLearning.Repositories;

namespace FlashcardLearning.Services
{
    public class FolderService : IFolderService
    {
        private readonly IFolderRepository _folderRepository;
        private readonly IDeckRepository _deckRepository;

        public FolderService(IFolderRepository folderRepository, IDeckRepository deckRepository)
        {
            _folderRepository = folderRepository;
            _deckRepository = deckRepository;
        }

        public async Task<IEnumerable<FolderResponse>> GetFoldersForUserAsync(Guid userId)
        {
            var folders = await _folderRepository.GetFoldersWithDecksAsync(userId);
            
            return folders.Select(f => new FolderResponse
            {
                Id = f.Id,
                Name = f.Name,
                Description = f.Description,
                UserId = f.UserId,
                CreatedAt = f.CreatedAt,
                DeckCount = f.Decks?.Count ?? 0
            });
        }

        public async Task<FolderWithDecksResponse?> GetFolderByIdAsync(Guid folderId, Guid userId)
        {
            var folder = await _folderRepository.GetFolderWithDecksAndFlashcardsAsync(folderId);
            
            if (folder == null)
            {
                return null;
            }

            // Check ownership
            if (folder.UserId != userId)
            {
                throw new UnauthorizedAccessException("You do not have permission to access this folder.");
            }

            return new FolderWithDecksResponse
            {
                Id = folder.Id,
                Name = folder.Name,
                Description = folder.Description,
                UserId = folder.UserId,
                CreatedAt = folder.CreatedAt,
                Decks = folder.Decks?.Select(d => new DeckSummary
                {
                    Id = d.Id,
                    Title = d.Title,
                    Description = d.Description,
                    IsPublic = d.IsPublic,
                    CreatedAt = d.CreatedAt,
                    FlashcardCount = d.Flashcards?.Count ?? 0
                }).ToList() ?? new List<DeckSummary>()
            };
        }

        public async Task<FolderResponse> CreateFolderAsync(CreateFolderRequest request, Guid userId)
        {
            // Check for duplicate folder name
            var existingFolder = await _folderRepository.GetFolderByNameAsync(request.Name, userId);
            if (existingFolder != null)
            {
                throw new InvalidOperationException("Folder is already exist.");
            }

            var folder = new Folder
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                UserId = userId,
                CreatedAt = DateTime.Now
            };

            await _folderRepository.AddAsync(folder);

            return new FolderResponse
            {
                Id = folder.Id,
                Name = folder.Name,
                Description = folder.Description,
                UserId = folder.UserId,
                CreatedAt = folder.CreatedAt,
                DeckCount = 0
            };
        }

        public async Task<bool> UpdateFolderAsync(Guid folderId, CreateFolderRequest request, Guid userId)
        {
            var folder = await _folderRepository.GetByIdAsync(folderId);
            
            if (folder == null)
            {
                return false;
            }

            // Check ownership
            if (folder.UserId != userId)
            {
                throw new UnauthorizedAccessException("You do not have permission to update this folder.");
            }

            // Check for duplicate name with other folders
            var duplicateFolder = await _folderRepository.GetFolderByNameAsync(request.Name, userId);
            if (duplicateFolder != null && duplicateFolder.Id != folderId)
            {
                throw new InvalidOperationException("Folder is already exist.");
            }

            folder.Name = request.Name;
            folder.Description = request.Description;

            await _folderRepository.UpdateAsync(folder);

            return true;
        }

        public async Task<bool> DeleteFolderAsync(Guid folderId, Guid userId, bool isAdmin)
        {
            var folder = await _folderRepository.GetFolderWithDecksAsync(folderId);
            
            if (folder == null)
            {
                return false;
            }

            // Check ownership or admin
            if (folder.UserId != userId && !isAdmin)
            {
                throw new UnauthorizedAccessException("You do not have permission to delete this folder.");
            }

            // Set FolderId = null for all decks in the folder
            if (folder.Decks != null && folder.Decks.Any())
            {
                foreach (var deck in folder.Decks)
                {
                    deck.FolderId = null;
                    await _deckRepository.UpdateAsync(deck);
                }
            }

            await _folderRepository.DeleteAsync(folder);

            return true;
        }

        public async Task<IEnumerable<DeckSummary>> GetUnassignedDecksAsync(Guid userId)
        {
            var decks = await _deckRepository.GetUnassignedDecksAsync(userId);
            
            return decks.Select(d => new DeckSummary
            {
                Id = d.Id,
                Title = d.Title,
                Description = d.Description,
                IsPublic = d.IsPublic,
                CreatedAt = d.CreatedAt,
                FlashcardCount = d.Flashcards?.Count ?? 0
            });
        }
    }
}
