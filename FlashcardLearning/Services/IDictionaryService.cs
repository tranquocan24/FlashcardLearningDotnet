namespace FlashcardLearning.Services
{
    /// <summary>
    /// Interface cho Dictionary Service
    /// </summary>
    public interface IDictionaryService
    {
        /// <summary>
        /// Tra c?u ngh?a ti?ng Vi?t c?a t? ti?ng Anh
        /// ?u tiên l?y t? cache, n?u không có thì g?i API external
        /// </summary>
        Task<string> LookupWordAsync(string word);

        /// <summary>
        /// L?y URL audio phát âm c?a t? ti?ng Anh
        /// </summary>
        Task<string?> GetAudioUrlAsync(string word);
    }
}
