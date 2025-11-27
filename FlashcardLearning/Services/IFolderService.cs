using FlashcardLearning.DTOs;

namespace FlashcardLearning.Services
{
    public interface IFolderService
    {
        Task<IEnumerable<FolderResponse>> GetFoldersForUserAsync(Guid userId);
        Task<FolderWithDecksResponse?> GetFolderByIdAsync(Guid folderId, Guid userId);
        Task<FolderResponse> CreateFolderAsync(CreateFolderRequest request, Guid userId);
        Task<bool> UpdateFolderAsync(Guid folderId, CreateFolderRequest request, Guid userId);
        Task<bool> DeleteFolderAsync(Guid folderId, Guid userId, bool isAdmin);
        Task<IEnumerable<DeckSummary>> GetUnassignedDecksAsync(Guid userId);
    }
}
