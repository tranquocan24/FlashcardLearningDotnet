using System.Text.Json;

namespace FlashcardLearning.Services
{
    public class DictionaryService
    {
        private readonly HttpClient _httpClient;

        public DictionaryService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string?> GetAudioUrlAsync(string word)
        {
            try
            {
                // Gọi API miễn phí
                var url = $"https://api.dictionaryapi.dev/api/v2/entries/en/{word}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode) return null;

                var jsonString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(jsonString);
                var root = doc.RootElement;

                // Cấu trúc JSON trả về là mảng, lấy phần tử đầu tiên
                if (root.GetArrayLength() > 0)
                {
                    var firstEntry = root[0];
                    if (firstEntry.TryGetProperty("phonetics", out var phonetics))
                    {
                        foreach (var phone in phonetics.EnumerateArray())
                        {
                            // Tìm cái nào có chứa audio
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
            catch
            {
                // Nếu lỗi mạng hoặc không tìm thấy thì trả về null (chấp nhận không có tiếng)
                return null;
            }
        }
    }
}