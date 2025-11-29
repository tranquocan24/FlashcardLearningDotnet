using FlashcardLearning.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlashcardLearning.Controllers
{

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
                    return BadRequest(new { message = "Word is null or have white space" });
                }

                if (word.Length > 200)
                {
                    return BadRequest(new { message = "Word is too long (maximum 200 characters)" });
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
                return StatusCode(500, new { message = "Error while searching for the word." });
            }
        }

        public class LookupResponse
        {
            public string Word { get; set; } = string.Empty;
            public string Meaning { get; set; } = string.Empty;
            public bool Success { get; set; }
        }
    }
}
