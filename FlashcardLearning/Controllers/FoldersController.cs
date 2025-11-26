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
    public class FoldersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FoldersController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// L?y danh sách t?t c? th? m?c c?a user hi?n t?i
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FolderResponse>>> GetFolders()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            var folders = await _context.Folders
                .Where(f => f.UserId.ToString() == currentUserId)
                .Include(f => f.Decks)
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => new FolderResponse
                {
                    Id = f.Id,
                    Name = f.Name,
                    Description = f.Description,
                    UserId = f.UserId,
                    CreatedAt = f.CreatedAt,
                    DeckCount = f.Decks.Count
                })
                .ToListAsync();

            return Ok(folders);
        }

        /// <summary>
        /// L?y chi ti?t m?t th? m?c kèm danh sách Decks bên trong
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<FolderWithDecksResponse>> GetFolder(Guid id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            var folder = await _context.Folders
                .Where(f => f.Id == id)
                .Include(f => f.Decks)
                    .ThenInclude(d => d.Flashcards)
                .FirstOrDefaultAsync();

            if (folder == null)
            {
                return NotFound("Không tìm th?y th? m?c.");
            }

            // Ki?m tra quy?n s? h?u
            if (folder.UserId.ToString() != currentUserId)
            {
                return Forbid("B?n không có quy?n truy c?p th? m?c này.");
            }

            var response = new FolderWithDecksResponse
            {
                Id = folder.Id,
                Name = folder.Name,
                Description = folder.Description,
                UserId = folder.UserId,
                CreatedAt = folder.CreatedAt,
                Decks = folder.Decks.Select(d => new DeckSummary
                {
                    Id = d.Id,
                    Title = d.Title,
                    Description = d.Description,
                    IsPublic = d.IsPublic,
                    CreatedAt = d.CreatedAt,
                    FlashcardCount = d.Flashcards?.Count ?? 0
                }).ToList()
            };

            return Ok(response);
        }

        /// <summary>
        /// T?o th? m?c m?i
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<FolderResponse>> CreateFolder(CreateFolderRequest request)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            // Ki?m tra trùng tên th? m?c cho cùng m?t user
            var existingFolder = await _context.Folders
                .FirstOrDefaultAsync(f => f.Name == request.Name && f.UserId.ToString() == currentUserId);

            if (existingFolder != null)
            {
                return BadRequest("Th? m?c v?i tên này ?ã t?n t?i.");
            }

            var folder = new Folder
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                UserId = Guid.Parse(currentUserId),
                CreatedAt = DateTime.UtcNow
            };

            _context.Folders.Add(folder);
            await _context.SaveChangesAsync();

            var response = new FolderResponse
            {
                Id = folder.Id,
                Name = folder.Name,
                Description = folder.Description,
                UserId = folder.UserId,
                CreatedAt = folder.CreatedAt,
                DeckCount = 0
            };

            return CreatedAtAction(nameof(GetFolder), new { id = folder.Id }, response);
        }

        /// <summary>
        /// C?p nh?t thông tin th? m?c
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFolder(Guid id, CreateFolderRequest request)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            var folder = await _context.Folders.FindAsync(id);
            if (folder == null)
            {
                return NotFound("Không tìm th?y th? m?c.");
            }

            // Ki?m tra quy?n s? h?u
            if (folder.UserId.ToString() != currentUserId)
            {
                return Forbid("B?n không có quy?n ch?nh s?a th? m?c này.");
            }

            // Ki?m tra trùng tên v?i th? m?c khác c?a cùng user
            var duplicateFolder = await _context.Folders
                .FirstOrDefaultAsync(f => f.Name == request.Name 
                    && f.UserId.ToString() == currentUserId 
                    && f.Id != id);

            if (duplicateFolder != null)
            {
                return BadRequest("Th? m?c v?i tên này ?ã t?n t?i.");
            }

            folder.Name = request.Name;
            folder.Description = request.Description;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await FolderExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Xóa th? m?c (các Deck bên trong s? có FolderId = NULL)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFolder(Guid id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            var folder = await _context.Folders
                .Include(f => f.Decks)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (folder == null)
            {
                return NotFound("Không tìm th?y th? m?c.");
            }

            // Ki?m tra quy?n (owner ho?c admin)
            if (folder.UserId.ToString() != currentUserId && currentUserRole != UserRoles.Admin)
            {
                return Forbid("B?n không có quy?n xóa th? m?c này.");
            }

            // Set FolderId = null cho t?t c? các Deck trong th? m?c
            if (folder.Decks != null && folder.Decks.Any())
            {
                foreach (var deck in folder.Decks)
                {
                    deck.FolderId = null;
                }
            }

            _context.Folders.Remove(folder);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// L?y danh sách Decks không thu?c th? m?c nào
        /// </summary>
        [HttpGet("unassigned-decks")]
        public async Task<ActionResult<IEnumerable<DeckSummary>>> GetUnassignedDecks()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            var decks = await _context.Decks
                .Where(d => d.UserId.ToString() == currentUserId && d.FolderId == null)
                .Include(d => d.Flashcards)
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => new DeckSummary
                {
                    Id = d.Id,
                    Title = d.Title,
                    Description = d.Description,
                    IsPublic = d.IsPublic,
                    CreatedAt = d.CreatedAt,
                    FlashcardCount = d.Flashcards != null ? d.Flashcards.Count : 0
                })
                .ToListAsync();

            return Ok(decks);
        }

        private async Task<bool> FolderExists(Guid id)
        {
            return await _context.Folders.AnyAsync(e => e.Id == id);
        }
    }
}
