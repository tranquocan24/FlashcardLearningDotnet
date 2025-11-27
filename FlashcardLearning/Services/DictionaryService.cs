using FlashcardLearning.Models;
using FlashcardLearning.Repositories;
using System.Text.Json;

namespace FlashcardLearning.Services
{
    public class DictionaryService : IDictionaryService
    {
        private readonly HttpClient _httpClient;
        private readonly IDictionaryRepository _dictionaryRepository;
        private readonly ILogger<DictionaryService> _logger;

        public DictionaryService(
            HttpClient httpClient,
            IDictionaryRepository dictionaryRepository,
            ILogger<DictionaryService> logger)
        {
            _httpClient = httpClient;
            _dictionaryRepository = dictionaryRepository;
            _logger = logger;
            // Set timeout to prevent hanging
            _httpClient.Timeout = TimeSpan.FromSeconds(5);
        }

        /// <summary>
        /// Tra cứu nghĩa tiếng Việt của từ tiếng Anh
        /// </summary>
        public async Task<string> LookupWordAsync(string word)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(word))
                return string.Empty;

            var normalizedWord = word.Trim().ToLower();

            try
            {
                // 1. Kiểm tra trong cache (Database)
                var cachedEntry = await _dictionaryRepository.GetByWordAsync(normalizedWord);
                if (cachedEntry != null)
                {
                    _logger.LogInformation($"Cache hit for word: {normalizedWord}");
                    return cachedEntry.Meaning;
                }

                // 2. Gọi API bên thứ 3 (MyMemory Translated)
                _logger.LogInformation($"Cache miss for word: {normalizedWord}. Calling external API...");
                var meaning = await FetchFromExternalApiAsync(normalizedWord);

                if (!string.IsNullOrEmpty(meaning))
                {
                    // 3. Lưu vào cache cho lần sau
                    var newEntry = new DictionaryEntry
                    {
                        Word = normalizedWord,
                        Meaning = meaning
                    };

                    await _dictionaryRepository.AddAsync(newEntry);
                    _logger.LogInformation($"Cached new word: {normalizedWord}");
                }

                return meaning;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error looking up word: {normalizedWord}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Gọi API MyMemory để lấy nghĩa tiếng Việt
        /// </summary>
        private async Task<string> FetchFromExternalApiAsync(string word)
        {
            try
            {
                var url = $"https://api.mymemory.translated.net/get?q={Uri.EscapeDataString(word)}&langpair=en|vi";

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                var response = await _httpClient.GetAsync(url, cts.Token);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"External API returned status code: {response.StatusCode}");
                    return string.Empty;
                }

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<MyMemoryResponse>(json);

                if (result?.ResponseData?.TranslatedText != null)
                {
                    return result.ResponseData.TranslatedText;
                }

                _logger.LogWarning($"No translation found for word: {word}");
                return string.Empty;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning($"Translation API timeout for word: {word}");
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calling external API for word: {word}");
                return string.Empty;
            }
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

        /// <summary>
        /// DTO cho MyMemory API Response
        /// </summary>
        private class MyMemoryResponse
        {
            public ResponseDataModel? ResponseData { get; set; }

            public class ResponseDataModel
            {
                public string? TranslatedText { get; set; }
            }
        }
    }
}