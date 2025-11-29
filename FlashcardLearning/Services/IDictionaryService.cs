namespace FlashcardLearning.Services
{
    public interface IDictionaryService
    {
        Task<string> LookupWordAsync(string word);
        Task<string?> GetAudioUrlAsync(string word);
    }
}
