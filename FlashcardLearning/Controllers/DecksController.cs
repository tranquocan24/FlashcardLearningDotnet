using FlashcardLearning.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FlashcardLearning.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DecksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DecksController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Deck>>> GetDecks()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            return await _context.Decks
                .Include(d => d.Flashcards)
                .Where(d => d.UserId.ToString() == currentUserId || d.IsPublic == true)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Deck>> CreateDeck(Deck deck)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            deck.UserId = Guid.Parse(currentUserId);

            deck.Id = Guid.NewGuid();
            deck.CreatedAt = DateTime.Now;

            _context.Decks.Add(deck);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDecks), new { id = deck.Id }, deck);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDeck(Guid id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var deck = await _context.Decks.FindAsync(id);
            if (deck == null) return NotFound();

            if (deck.UserId.ToString() != currentUserId)
            {
                return Forbid();
            }

            _context.Decks.Remove(deck);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
