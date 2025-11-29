using FlashcardLearning.Constants;
using FlashcardLearning.DTOs;
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
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("profile")]
        public async Task<ActionResult<object>> GetProfile()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            var profile = await _userService.GetProfileAsync(Guid.Parse(currentUserId));
            
            if (profile == null)
            {
                return NotFound();
            }

            return Ok(profile);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(User userUpdate)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            var success = await _userService.UpdateProfileAsync(
                Guid.Parse(currentUserId), 
                userUpdate.Username, 
                userUpdate.AvatarUrl);

            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto request)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            try
            {
                var success = await _userService.ChangePasswordAsync(
                    Guid.Parse(currentUserId), 
                    request.OldPassword, 
                    request.NewPassword);

                if (!success)
                {
                    return NotFound();
                }

                return Ok(new { message = "Đổi mật khẩu thành công!" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //ADMIN ENDPOINTS

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAllUsers()
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            
            if (role != UserRoles.Admin)
            {
                return Forbid();
            }

            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpPut("admin/update-user")]
        public async Task<ActionResult> AdminUpdateUser(AdminUpdateUserDto request)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            
            if (role != UserRoles.Admin)
            {
                return Forbid();
            }

            try
            {
                var result = await _userService.AdminUpdateUserAsync(
                    request.Email, 
                    request.NewEmail, 
                    request.NewPassword);

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (role != UserRoles.Admin)
            {
                return Forbid();
            }

            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            try
            {
                var success = await _userService.DeleteUserAsync(id, Guid.Parse(currentUserId));
                
                if (!success)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}