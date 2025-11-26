using FlashcardLearning.Constants;
using FlashcardLearning.Models;
using FlashcardLearning.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;
using System.Security.Claims;

namespace FlashcardLearning.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FlashcardsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly DictionaryService _dictionaryService;
        private readonly ILogger<FlashcardsController> _logger;

        public FlashcardsController(AppDbContext context, DictionaryService dictionaryService, ILogger<FlashcardsController> logger)
        {
            _context = context;
            _dictionaryService = dictionaryService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Flashcard>> GetFlashcard(Guid id)
        {
            var card = await _context.Flashcards
                                     .Include(f => f.Deck)
                                     .FirstOrDefaultAsync(f => f.Id == id);

            if (card == null) return NotFound("Flashcard not found.");

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            if (card.Deck != null &&
                !card.Deck.IsPublic &&
                card.Deck.UserId.ToString() != currentUserId &&
                currentUserRole != UserRoles.Admin)
            {
                return Forbid();
            }

            return card;
        }

        [HttpPost]
        public async Task<ActionResult<Flashcard>> CreateFlashcard(Flashcard flashcard)
        {
            try
            {
                _logger.LogInformation("Creating flashcard with term: {Term}", flashcard.Term);

                // Validate deck exists
                var deck = await _context.Decks.FindAsync(flashcard.DeckId);
                if (deck == null)
                {
                    return NotFound("Deck not found.");
                }

                // Check permissions
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

                if (deck.UserId?.ToString() != currentUserId && currentUserRole != UserRoles.Admin)
                {
                    return Forbid();
                }

                // Prepare data
                flashcard.Id = Guid.NewGuid();
                flashcard.Example = flashcard.Example ?? "";
                flashcard.ImageUrl = flashcard.ImageUrl ?? "";

                // Auto-generate audio (with timeout protection)
                if (string.IsNullOrEmpty(flashcard.AudioUrl))
                {
                    try
                    {
                        _logger.LogInformation("Fetching audio for term: {Term}", flashcard.Term);
                        
                        string? autoAudioUrl = await _dictionaryService.GetAudioUrlAsync(flashcard.Term);
                        flashcard.AudioUrl = autoAudioUrl ?? "";
                        
                        if (!string.IsNullOrEmpty(autoAudioUrl))
                        {
                            _logger.LogInformation("Audio found for term: {Term}", flashcard.Term);
                        }
                        else
                        {
                            _logger.LogInformation("No audio found for term: {Term}", flashcard.Term);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Don't fail the whole request if audio fetch fails
                        _logger.LogWarning(ex, "Failed to fetch audio for term: {Term}", flashcard.Term);
                        flashcard.AudioUrl = "";
                    }
                }

                // Save to database
                _context.Flashcards.Add(flashcard);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Flashcard created successfully with ID: {Id}", flashcard.Id);

                return CreatedAtAction(nameof(GetFlashcard), new { id = flashcard.Id }, flashcard);
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
            if (id != flashcardUpdate.Id) return BadRequest("ID mismatch.");

            var existingCard = await _context.Flashcards
                                             .Include(f => f.Deck)
                                             .FirstOrDefaultAsync(f => f.Id == id);

            if (existingCard == null) return NotFound("Flashcard not found.");

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            if (existingCard.Deck != null &&
                existingCard.Deck.UserId.ToString() != currentUserId &&
                currentUserRole != UserRoles.Admin)
            {
                return Forbid();
            }

            existingCard.Term = flashcardUpdate.Term;
            existingCard.Definition = flashcardUpdate.Definition;
            existingCard.Example = flashcardUpdate.Example;
            existingCard.ImageUrl = flashcardUpdate.ImageUrl;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FlashcardExists(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFlashcard(Guid id)
        {
            var card = await _context.Flashcards
                                     .Include(f => f.Deck)
                                     .FirstOrDefaultAsync(f => f.Id == id);

            if (card == null) return NotFound("Flashcard not found.");

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            if (card.Deck != null &&
                card.Deck.UserId.ToString() != currentUserId &&
                currentUserRole != UserRoles.Admin)
            {
                return Forbid();
            }

            _context.Flashcards.Remove(card);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool FlashcardExists(Guid id)
        {
            return _context.Flashcards.Any(e => e.Id == id);
        }
    }
}