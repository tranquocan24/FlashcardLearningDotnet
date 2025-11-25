using FlashcardLearning.Constants;
using FlashcardLearning.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FlashcardLearning.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FlashcardsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FlashcardsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Flashcard>> GetFlashcard(Guid id)
        {
            var card = await _context.Flashcards
                                     .Include(f => f.Deck)
                                     .FirstOrDefaultAsync(f => f.Id == id);

            if (card == null) return NotFound("Không tìm thấy thẻ.");

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
            var deck = await _context.Decks.FindAsync(flashcard.DeckId);
            if (deck == null)
            {
                return NotFound("Bộ thẻ (Deck) không tồn tại.");
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            if (deck.UserId.ToString() != currentUserId && currentUserRole != UserRoles.Admin)
            {
                return Forbid();
            }

            flashcard.Id = Guid.NewGuid();

            if (string.IsNullOrEmpty(flashcard.Example)) flashcard.Example = "";
            if (string.IsNullOrEmpty(flashcard.ImageUrl)) flashcard.ImageUrl = "";

            _context.Flashcards.Add(flashcard);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFlashcard), new { id = flashcard.Id }, flashcard);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFlashcard(Guid id, Flashcard flashcardUpdate)
        {
            if (id != flashcardUpdate.Id) return BadRequest("ID trên URL không khớp với ID trong dữ liệu.");

            var existingCard = await _context.Flashcards
                                             .Include(f => f.Deck)
                                             .FirstOrDefaultAsync(f => f.Id == id);

            if (existingCard == null) return NotFound("Thẻ không tồn tại.");

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

            if (card == null) return NotFound("Thẻ không tồn tại.");

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