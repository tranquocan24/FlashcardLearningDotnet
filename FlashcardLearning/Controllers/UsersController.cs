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
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("profile")]
        public async Task<ActionResult<object>> GetProfile()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _context.Users
                .Include(u => u.Decks)
                .FirstOrDefaultAsync(u => u.Id.ToString() == currentUserId);

            if (user == null) return NotFound();

            return Ok(new
            {
                user.Id,
                user.Username,
                user.Email,
                user.Role,
                user.AvatarUrl,
                user.CreatedAt,
                DeckCount = user.Decks?.Count ?? 0
            });
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(User userUpdate)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FindAsync(Guid.Parse(currentUserId));

            if (user == null) return NotFound();

            user.AvatarUrl = userUpdate.AvatarUrl;

            if (!string.IsNullOrEmpty(userUpdate.Username))
            {
                user.Username = userUpdate.Username;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto request)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FindAsync(Guid.Parse(currentUserId));

            if (user == null) return NotFound();

            if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.Password))
            {
                return BadRequest("Mật khẩu cũ không chính xác.");
            }

            string newPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.Password = newPasswordHash;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Đổi mật khẩu thành công!" });
        }

        //Admin
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAllUsers()
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            if (role != UserRoles.Admin) return Forbid();

            return await _context.Users
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Email,
                    u.Role,
                    u.CreatedAt
                })
                .ToListAsync();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            if (role != UserRoles.Admin) return Forbid();

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (user.Id.ToString() == currentUserId)
            {
                return BadRequest("Bạn không thể tự xóa tài khoản của chính mình.");
            }

            _context.Users.Remove(user);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}