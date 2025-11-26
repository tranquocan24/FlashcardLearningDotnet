using System.Text.Json;

namespace FlashcardLearning.Services
{
    public class DictionaryService
    {
        private readonly HttpClient _httpClient;

        public DictionaryService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            // Set timeout to prevent hanging
            _httpClient.Timeout = TimeSpan.FromSeconds(5);
        }

        public async Task<string?> GetAudioUrlAsync(string word)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(word)) return null;
                
                // Only process English words (simple check)
                word = word.Trim().ToLower();
                
                // Call free API with timeout
                var url = $"https://api.dictionaryapi.dev/api/v2/entries/en/{word}";
                
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                var response = await _httpClient.GetAsync(url, cts.Token);

                if (!response.IsSuccessStatusCode) return null;

                var jsonString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(jsonString);
                var root = doc.RootElement;

                // Parse JSON array response
                if (root.GetArrayLength() > 0)
                {
                    var firstEntry = root[0];
                    if (firstEntry.TryGetProperty("phonetics", out var phonetics))
                    {
                        foreach (var phone in phonetics.EnumerateArray())
                        {
                            // Find the one with audio
                            if (phone.TryGetProperty("audio", out var audio) &&
                                !string.IsNullOrEmpty(audio.GetString()))
                            {
                                return audio.GetString();
                            }
                        }
                    }
                }
                return null;
            }
            catch (OperationCanceledException)
            {
                // Timeout - return null instead of throwing
                Console.WriteLine($"Dictionary API timeout for word: {word}");
                return null;
            }
            catch (Exception ex)
            {
                // Any other error - log and return null
                Console.WriteLine($"Dictionary API error for word: {word}, Error: {ex.Message}");
                return null;
            }
        }
    }
}