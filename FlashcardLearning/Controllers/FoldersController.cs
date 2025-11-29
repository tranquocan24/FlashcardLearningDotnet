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
    public class FoldersController : ControllerBase
    {
        private readonly IFolderService _folderService;

        public FoldersController(IFolderService folderService)
        {
            _folderService = folderService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FolderResponse>>> GetFolders()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            var folders = await _folderService.GetFoldersForUserAsync(Guid.Parse(currentUserId));
            return Ok(folders);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FolderWithDecksResponse>> GetFolder(Guid id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            try
            {
                var folder = await _folderService.GetFolderByIdAsync(id, Guid.Parse(currentUserId));
                
                if (folder == null)
                {
                    return NotFound("Can not find folder.");
                }

                return Ok(folder);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }
        [HttpPost]
        public async Task<ActionResult<FolderResponse>> CreateFolder(CreateFolderRequest request)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            try
            {
                var folder = await _folderService.CreateFolderAsync(request, Guid.Parse(currentUserId));
                return CreatedAtAction(nameof(GetFolder), new { id = folder.Id }, folder);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFolder(Guid id, CreateFolderRequest request)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            try
            {
                var success = await _folderService.UpdateFolderAsync(id, request, Guid.Parse(currentUserId));
                
                if (!success)
                {
                    return NotFound("Can not find folder.");
                }

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFolder(Guid id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            try
            {
                bool isAdmin = currentUserRole == UserRoles.Admin;
                var success = await _folderService.DeleteFolderAsync(id, Guid.Parse(currentUserId), isAdmin);
                
                if (!success)
                {
                    return NotFound("Can not find folder.");
                }

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpGet("unassigned-decks")]
        public async Task<ActionResult<IEnumerable<DeckSummary>>> GetUnassignedDecks()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            var decks = await _folderService.GetUnassignedDecksAsync(Guid.Parse(currentUserId));
            return Ok(decks);
        }
    }
}
