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

        public FlashcardsController(AppDbContext context, DictionaryService dictionaryService)
        {
            _context = context;
            _dictionaryService = dictionaryService; 
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
            // --- PHẦN 1: KIỂM TRA DECK VÀ QUYỀN (Giữ nguyên code của bạn) ---
            var deck = await _context.Decks.FindAsync(flashcard.DeckId);
            if (deck == null)
            {
                return NotFound("Bộ thẻ (Deck) không tồn tại.");
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            // Lưu ý: Đảm bảo deck.UserId không null trước khi ToString() để tránh lỗi
            if (deck.UserId?.ToString() != currentUserId && currentUserRole != UserRoles.Admin)
            {
                return Forbid();
            }

            // --- PHẦN 2: CHUẨN BỊ DỮ LIỆU ---
            flashcard.Id = Guid.NewGuid();

            // Xử lý null cho các trường string (Giữ nguyên code của bạn)
            if (string.IsNullOrEmpty(flashcard.Example)) flashcard.Example = "";
            if (string.IsNullOrEmpty(flashcard.ImageUrl)) flashcard.ImageUrl = "";

            // --- PHẦN 3: TỰ ĐỘNG LẤY AUDIO (Code mới thêm vào) ---
            // Chỉ gọi API nếu client chưa gửi link Audio lên
            if (string.IsNullOrEmpty(flashcard.AudioUrl))
            {
                // Gọi service lấy link mp3 từ từ điển
                string? autoAudioUrl = await _dictionaryService.GetAudioUrlAsync(flashcard.Term);

                // Nếu tìm thấy thì gán vào, không thấy thì để rỗng hoặc null
                flashcard.AudioUrl = autoAudioUrl ?? "";
            }

            // --- PHẦN 4: LƯU VÀO DATABASE ---
            _context.Flashcards.Add(flashcard);
            await _context.SaveChangesAsync();

            // Trả về kết quả
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