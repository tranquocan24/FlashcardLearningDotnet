using FlashcardLearning.Constants;
using FlashcardLearning.Models;
using FlashcardLearning.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FlashcardLearning.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FlashcardsController : ControllerBase
    {
        private readonly IFlashcardService _flashcardService;
        private readonly ILogger<FlashcardsController> _logger;

        public FlashcardsController(IFlashcardService flashcardService, ILogger<FlashcardsController> logger)
        {
            _flashcardService = flashcardService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Flashcard>> GetFlashcard(Guid id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
            
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            try
            {
                bool isAdmin = currentUserRole == UserRoles.Admin;
                var flashcard = await _flashcardService.GetFlashcardAsync(id, Guid.Parse(currentUserId), isAdmin);

                if (flashcard == null)
                {
                    return NotFound("Flashcard not found.");
                }

                return Ok(flashcard);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<Flashcard>> CreateFlashcard(Flashcard flashcard)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
            
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            try
            {
                bool isAdmin = currentUserRole == UserRoles.Admin;
                var createdFlashcard = await _flashcardService.CreateFlashcardAsync(
                    flashcard, 
                    Guid.Parse(currentUserId), 
                    isAdmin);

                return CreatedAtAction(nameof(GetFlashcard), new { id = createdFlashcard.Id }, createdFlashcard);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating flashcard");
                return StatusCode(500, new { message = "Error creating flashcard", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFlashcard(Guid id, Flashcard flashcardUpdate)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
            
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            try
            {
                bool isAdmin = currentUserRole == UserRoles.Admin;
                var success = await _flashcardService.UpdateFlashcardAsync(
                    id, 
                    flashcardUpdate, 
                    Guid.Parse(currentUserId), 
                    isAdmin);

                if (!success)
                {
                    return NotFound("Flashcard not found.");
                }

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFlashcard(Guid id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
            
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            try
            {
                bool isAdmin = currentUserRole == UserRoles.Admin;
                var success = await _flashcardService.DeleteFlashcardAsync(
                    id, 
                    Guid.Parse(currentUserId), 
                    isAdmin);

                if (!success)
                {
                    return NotFound("Flashcard not found.");
                }

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }
    }
}