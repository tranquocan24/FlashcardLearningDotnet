using FlashcardLearning.Models;

namespace FlashcardLearning.Repositories;

public interface IFolderRepository : IRepository<Folder>
{
    Task<Folder?> GetFolderWithDecksAsync(Guid folderId);
    Task<Folder?> GetFolderWithDecksAndFlashcardsAsync(Guid folderId);
    Task<IEnumerable<Folder>> GetFoldersByUserIdAsync(Guid userId);
    Task<IEnumerable<Folder>> GetFoldersWithDecksAsync(Guid userId);
    Task<Folder?> GetFolderByNameAsync(string name, Guid userId);
    Task<bool> IsUserOwnerAsync(Guid folderId, Guid userId);
}
