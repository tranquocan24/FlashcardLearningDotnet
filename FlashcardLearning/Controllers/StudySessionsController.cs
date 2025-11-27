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
    public class StudySessionsController : ControllerBase
    {
        private readonly IStudySessionService _studySessionService;

        public StudySessionsController(IStudySessionService studySessionService)
        {
            _studySessionService = studySessionService;
        }

        [HttpPost]
        public async Task<ActionResult<StudySession>> CreateSession(StudySession session)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            try
            {
                var createdSession = await _studySessionService.CreateSessionAsync(
                    session, 
                    Guid.Parse(currentUserId));

                return Ok(createdSession);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Xem lịch sử học tập của chính mình
        /// </summary>
        [HttpGet("history")]
        public async Task<ActionResult<IEnumerable<object>>> GetMyHistory()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            var history = await _studySessionService.GetMyHistoryAsync(Guid.Parse(currentUserId));
            return Ok(history);
        }

        /// <summary>
        /// Xem bảng xếp hạng của một Bộ thẻ
        /// </summary>
        [HttpGet("leaderboard/{deckId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetDeckLeaderboard(Guid deckId)
        {
            try
            {
                var leaderboard = await _studySessionService.GetLeaderboardAsync(deckId);
                return Ok(leaderboard);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// [ADMIN ONLY] Xem toàn bộ lịch sử hệ thống
        /// </summary>
        [HttpGet("admin/all-history")]
        public async Task<ActionResult<IEnumerable<StudySession>>> GetAllHistory()
        {
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
            
            if (currentUserRole != UserRoles.Admin)
            {
                return Forbid();
            }

            var history = await _studySessionService.GetAllHistoryAsync();
            return Ok(history);
        }

        /// <summary>
        /// [ADMIN ONLY] Xóa lịch sử
        /// </summary>
        [HttpDelete("admin/{id}")]
        public async Task<IActionResult> DeleteSession(Guid id)
        {
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
            
            if (currentUserRole != UserRoles.Admin)
            {
                return Forbid();
            }

            var success = await _studySessionService.DeleteSessionAsync(id);
            
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}