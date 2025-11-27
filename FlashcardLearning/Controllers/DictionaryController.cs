using FlashcardLearning.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlashcardLearning.Controllers
{
    /// <summary>
    /// API Controller cho ch?c n?ng tra t? ?i?n En -> Vi
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DictionaryController : ControllerBase
    {
        private readonly IDictionaryService _dictionaryService;
        private readonly ILogger<DictionaryController> _logger;

        public DictionaryController(
            IDictionaryService dictionaryService,
            ILogger<DictionaryController> logger)
        {
            _dictionaryService = dictionaryService;
            _logger = logger;
        }

        /// <summary>
        /// Tra c?u ngh?a ti?ng Vi?t c?a t? ti?ng Anh
        /// </summary>
        /// <param name="word">T? ti?ng Anh c?n tra</param>
        /// <returns>Ngh?a ti?ng Vi?t</returns>
        /// <response code="200">Tr? v? ngh?a ti?ng Vi?t</response>
        /// <response code="400">T? không h?p l?</response>
        [HttpGet("lookup")]
        [ProducesResponseType(typeof(LookupResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> LookupWord([FromQuery] string word)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(word))
                {
                    return BadRequest(new { message = "T? c?n tra không ???c ?? tr?ng" });
                }

                if (word.Length > 200)
                {
                    return BadRequest(new { message = "T? quá dài (t?i ?a 200 ký t?)" });
                }

                // Call service
                var meaning = await _dictionaryService.LookupWordAsync(word);

                return Ok(new LookupResponse
                {
                    Word = word.Trim().ToLower(),
                    Meaning = meaning,
                    Success = !string.IsNullOrEmpty(meaning)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in LookupWord for word: {word}");
                return StatusCode(500, new { message = "L?i khi tra t?" });
            }
        }

        /// <summary>
        /// DTO cho response
        /// </summary>
        public class LookupResponse
        {
            public string Word { get; set; } = string.Empty;
            public string Meaning { get; set; } = string.Empty;
            public bool Success { get; set; }
        }
    }
}
