using FlashcardLearning.Constants;
using FlashcardLearning.DTOs;
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
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            var query = _context.Decks
                .Include(d => d.Flashcards)
                .AsQueryable();

            if (currentUserRole != UserRoles.Admin)
            {
                query = query.Where(d => d.UserId.ToString() == currentUserId || d.IsPublic == true);
            }
            return await query
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Deck>> GetDeck(Guid id)
        {
            var deck = await _context.Decks
                                     .Include(d => d.Flashcards)
                                     .FirstOrDefaultAsync(d => d.Id == id);

            if (deck == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            if (deck.IsPublic == false &&
                deck.UserId.ToString() != currentUserId &&
                currentUserRole != UserRoles.Admin)
            {
                return Forbid();
            }

            return deck;
        }

        [HttpPost]
        public async Task<ActionResult<Deck>> CreateDeck(CreateDeckRequest request)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            // Kiểm tra FolderId nếu được cung cấp
            if (request.FolderId.HasValue)
            {
                var folder = await _context.Folders.FindAsync(request.FolderId.Value);
                if (folder == null)
                {
                    return BadRequest("Thư mục không tồn tại.");
                }

                // Kiểm tra folder có thuộc về user hiện tại không
                if (folder.UserId.ToString() != currentUserId)
                {
                    return Forbid("Bạn không có quyền thêm deck vào thư mục này.");
                }
            }

            var deck = new Deck
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description ?? string.Empty,
                IsPublic = request.IsPublic,
                UserId = Guid.Parse(currentUserId),
                FolderId = request.FolderId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Decks.Add(deck);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDeck), new { id = deck.Id }, deck);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDeck(Guid id, UpdateDeckRequest request)
        {
            var existingDeck = await _context.Decks.FindAsync(id);
            if (existingDeck == null) return NotFound("Không tìm thấy bộ thẻ.");

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            // Cho phép nếu là Chủ sở hữu HOẶC là Admin
            if (existingDeck.UserId.ToString() != currentUserId && currentUserRole != UserRoles.Admin)
            {
                return Forbid();
            }

            // Kiểm tra FolderId nếu được cung cấp
            if (request.FolderId.HasValue)
            {
                var folder = await _context.Folders.FindAsync(request.FolderId.Value);
                if (folder == null)
                {
                    return BadRequest("Thư mục không tồn tại.");
                }

                // Kiểm tra folder có thuộc về user hiện tại không
                if (folder.UserId.ToString() != currentUserId && currentUserRole != UserRoles.Admin)
                {
                    return Forbid("Bạn không có quyền chuyển deck vào thư mục này.");
                }
            }

            existingDeck.Title = request.Title;
            existingDeck.Description = request.Description ?? string.Empty;
            existingDeck.IsPublic = request.IsPublic;
            existingDeck.FolderId = request.FolderId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DeckExists(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDeck(Guid id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            var deck = await _context.Decks.FindAsync(id);
            if (deck == null) return NotFound();

            if (deck.UserId.ToString() != currentUserId && currentUserRole != UserRoles.Admin)
            {
                return Forbid();
            }

            _context.Decks.Remove(deck);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DeckExists(Guid id)
        {
            return _context.Decks.Any(e => e.Id == id);
        }
    }
}