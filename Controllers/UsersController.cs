using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexus.Backend.Data;
using Nexus.Backend.Models;

namespace Nexus.Backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly NexusDbContext _context;

        public UsersController(NexusDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all users with their roles. 
        /// Restricted to Rank 100 (Admin).
        /// </summary>
        [HttpGet("list")]
        public async Task<IActionResult> ListUsers(int page = 1, int pageSize = 10, string search = "")
        {
            try
            {
                // 1. Create a base query
                var query = _context.Users.Include(u => u.Role).AsQueryable();

                // 2. Apply Search Filter (Case-insensitive by default in SQL Server)
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(u =>
                        u.FullName.Contains(search) ||
                        u.Email.Contains(search) ||
                        u.Role.Name.Contains(search)
                    );
                }

                // 3. Get Total Count for the specific search (before pagination)
                var totalCount = await query.CountAsync();

                // 4. Execute Pagination
                var users = await query
                    .OrderByDescending(u => u.Role.Rank) // Keeps hierarchy order
                    .ThenBy(u => u.FullName)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new
                    {
                        u.Id,
                        u.FullName,
                        u.Email,
                        RoleName = u.Role.Name,
                        Rank = u.Role.Rank
                    })
                    .ToListAsync();

                // 5. Return metadata along with the items
                return Ok(new
                {
                    items = users,
                    totalCount,
                    pageNumber = page,
                    pageSize,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Admin provisions a new user manually.
        /// </summary>
        [HttpPost("provision")]
        public async Task<IActionResult> ProvisionUser([FromBody] CreateUserRequest request)
        {
            try
            {
                if (!IsSystemAdmin()) return StatusCode(403, "Only Admins can provision accounts.");

                // 1. Check if user already exists
                if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                    return BadRequest("User with this email already exists.");

                // 2. Create the new user
                var newUser = new User
                {
                    FullName = request.FullName,
                    Email = request.Email,
                    PasswordHash = request.Password, // In production: hash this!
                    RoleId = request.RoleId
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                return Ok(new { message = "User provisioned successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (!IsSystemAdmin()) return StatusCode(403, "Clearance level 100 required for deletion.");

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("Agent not found in the vault.");

            // Prevent deleting the last admin (optional safety check)
            if (user.Email == "admin@nexus.com")
                return BadRequest("The primary System Admin cannot be terminated.");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok("Agent successfully purged from the network.");
        }

        // --- Helper Logic ---
        private bool IsSystemAdmin()
        {
            var rankClaim = User.FindFirst("Rank")?.Value;
            return int.TryParse(rankClaim, out int rank) && rank >= 100;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] CreateUserRequest request)
        {
            if (!IsSystemAdmin()) return StatusCode(403, "Admin clearance required.");

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.FullName = request.FullName;
            user.Email = request.Email;
            user.RoleId = request.RoleId;

            // Only update password if a new one is provided
            if (!string.IsNullOrEmpty(request.Password))
            {
                user.PasswordHash = request.Password; // Remember to hash in production!
            }

            await _context.SaveChangesAsync();
            return Ok("Agent profile synchronized.");
        }
    }


    // DTO for User Creation
    public class CreateUserRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int RoleId { get; set; }
    }
}