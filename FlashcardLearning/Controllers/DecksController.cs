using FlashcardLearning.Constants;
using FlashcardLearning.DTOs;
using FlashcardLearning.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FlashcardLearning.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DecksController : ControllerBase
    {
        private readonly IDeckService _deckService;

        public DecksController(IDeckService deckService)
        {
            _deckService = deckService;
        }

        #region Helper Methods

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.Parse(userIdClaim!);
        }

        private bool IsCurrentUserAdmin()
        {
            return User.FindFirstValue(ClaimTypes.Role) == UserRoles.Admin;
        }

        #endregion

        #region Query Endpoints
        /// Admin: All decks, User: Own decks + Public decks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DeckResponse>>> GetDecks()
        {
            try
            {
                var userId = GetCurrentUserId();
                var isAdmin = IsCurrentUserAdmin();
                
                var decks = await _deckService.GetDecksForUserAsync(userId, isAdmin);
                
                return Ok(decks);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DeckDetailResponse>> GetDeck(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var isAdmin = IsCurrentUserAdmin();
                
                var deck = await _deckService.GetDeckByIdAsync(id, userId, isAdmin);
                
                if (deck == null)
                    return NotFound(new { message = "Deck not found or access denied" });
                
                return Ok(deck);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", details = ex.Message });
            }
        }

        #endregion

        #region Command Endpoints

        [HttpPost]
        public async Task<ActionResult<DeckResponse>> CreateDeck(CreateDeckRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                
                var deck = await _deckService.CreateDeckAsync(request, userId);
                
                return CreatedAtAction(nameof(GetDeck), new { id = deck.Id }, deck);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", details = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDeck(Guid id, UpdateDeckRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var isAdmin = IsCurrentUserAdmin();
                
                var success = await _deckService.UpdateDeckAsync(id, request, userId, isAdmin);
                
                if (!success)
                    return NotFound(new { message = "Deck not found" });
                
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", details = ex.Message });
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDeck(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var isAdmin = IsCurrentUserAdmin();
                
                var success = await _deckService.DeleteDeckAsync(id, userId, isAdmin);
                
                if (!success)
                    return NotFound(new { message = "Deck not found" });
                
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", details = ex.Message });
            }
        }

        #endregion
    }
}