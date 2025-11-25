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
    public class StudySessionsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StudySessionsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<StudySession>> CreateSession(StudySession session)
        {
            var deck = await _context.Decks.FindAsync(session.DeckId);
            if (deck == null) return NotFound("Bộ thẻ không tồn tại.");

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            session.UserId = Guid.Parse(currentUserId);

            session.Id = Guid.NewGuid();
            session.DateStudied = DateTime.Now;

            if (session.TotalCards > 0 && session.Score > session.TotalCards)
            {
                return BadRequest("Điểm số không thể lớn hơn tổng số câu.");
            }

            // Nếu Frontend quên gửi Mode, mặc định là "Flashcard"
            if (string.IsNullOrEmpty(session.Mode))
            {
                session.Mode = "Flashcard";
            }

            _context.StudySessions.Add(session);
            await _context.SaveChangesAsync();

            return Ok(session);
        }

        // =================================================================
        // 2. GET: Xem lịch sử học tập CỦA CHÍNH MÌNH
        // =================================================================
        [HttpGet("history")]
        public async Task<ActionResult<IEnumerable<object>>> GetMyHistory()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var sessions = await _context.StudySessions
                .Include(s => s.Deck) // Load tên bộ thẻ để hiển thị
                .Where(s => s.UserId.ToString() == currentUserId)
                .OrderByDescending(s => s.DateStudied) // Mới nhất lên đầu
                .Select(s => new
                {
                    s.Id,
                    s.DateStudied,
                    s.Score,
                    s.TotalCards,
                    s.Mode,
                    DeckTitle = s.Deck != null ? s.Deck.Title : "Bộ thẻ đã bị xóa", // Xử lý nếu Deck bị xóa
                    DeckId = s.DeckId
                })
                .ToListAsync();

            return Ok(sessions);
        }

        // =================================================================
        // 3. GET: Xem bảng xếp hạng của 1 Bộ thẻ (Ai học giỏi nhất?)
        // =================================================================
        [HttpGet("leaderboard/{deckId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetDeckLeaderboard(Guid deckId)
        {
            // Kiểm tra Deck có tồn tại không
            var deck = await _context.Decks.FindAsync(deckId);
            if (deck == null) return NotFound("Bộ thẻ không tồn tại.");

            // Logic lấy Top 10 người điểm cao nhất
            var leaderboard = await _context.StudySessions
                .Where(s => s.DeckId == deckId)
                .Include(s => s.User)
                .OrderByDescending(s => s.Score) // Điểm cao nhất
                .ThenBy(s => s.TotalCards)       // Nếu bằng điểm, ưu tiên ai học nhiều thẻ hơn (hoặc ít thời gian hơn tuỳ logic)
                .Take(10) // Lấy top 10
                .Select(s => new
                {
                    UserName = s.User != null ? s.User.Username : "Unknown",
                    Avatar = s.User != null ? s.User.AvatarUrl : null,
                    s.Score,
                    s.TotalCards,
                    Date = s.DateStudied
                })
                .ToListAsync();

            return Ok(leaderboard);
        }

        // =================================================================
        // 4. [ADMIN ONLY] GET: Xem toàn bộ lịch sử hệ thống
        // =================================================================
        [HttpGet("admin/all-history")]
        public async Task<ActionResult<IEnumerable<StudySession>>> GetAllHistory()
        {
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
            if (currentUserRole != UserRoles.Admin) return Forbid();

            return await _context.StudySessions
                .Include(s => s.Deck)
                .Include(s => s.User)
                .OrderByDescending(s => s.DateStudied)
                .ToListAsync();
        }

        // =================================================================
        // 5. [ADMIN ONLY] DELETE: Xóa lịch sử rác
        // =================================================================
        [HttpDelete("admin/{id}")]
        public async Task<IActionResult> DeleteSession(Guid id)
        {
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
            if (currentUserRole != UserRoles.Admin) return Forbid();

            var session = await _context.StudySessions.FindAsync(id);
            if (session == null) return NotFound();

            _context.StudySessions.Remove(session);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}