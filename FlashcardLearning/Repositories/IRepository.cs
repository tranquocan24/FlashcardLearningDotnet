namespace FlashcardLearning.Repositories;

/// <summary>
/// Generic repository interface for basic CRUD operations
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public interface IRepository<T> where T : class
{
    // Query operations
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate);
    
    // Command operations (with auto-save)
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    
    // Command operations (without auto-save - for transaction control)
    void Update(T entity);
    void Delete(T entity);
    Task<int> SaveChangesAsync();
    
    // Utility
    Task<bool> ExistsAsync(Guid id);
    Task<int> CountAsync();
}
