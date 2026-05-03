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
    public class RolesController : ControllerBase
    {
        private readonly NexusDbContext _context;
        public RolesController(NexusDbContext context) => _context = context;

        [HttpGet("list")]
        public async Task<IActionResult> GetRoles()
        {
            // Simple list for dropdowns and management
            var roles = await _context.Roles.OrderByDescending(r => r.Rank).ToListAsync();
            return Ok(roles);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateRole([FromBody] RoleDto request)
        {
            if (!IsAdmin()) return Forbid();

            if (await _context.Roles.AnyAsync(r => r.Name == request.Name))
                return BadRequest("Role already exists.");

            var role = new Role { Name = request.Name, Rank = request.Rank };
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Role created successfully", roleId = role.Id });
        }
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] RoleDto request)
        {
            try
            {
                if (!IsAdmin()) return Forbid();

                var role = await _context.Roles.FindAsync(id);
                if (role == null) return NotFound("Role not found.");

                // Optional: Check if the new name conflicts with another role
                if (await _context.Roles.AnyAsync(r => r.Name == request.Name && r.Id != id))
                    return BadRequest("Another role with this name already exists.");

                role.Name = request.Name;
                role.Rank = request.Rank;

                await _context.SaveChangesAsync();
                return Ok(new { message = "Role updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            if (!IsAdmin()) return Forbid();
            var role = await _context.Roles.FindAsync(id);
            if (role == null) return NotFound();

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Role deleted" });
        }

        private bool IsAdmin()
        {
            var rank = User.FindFirst("Rank")?.Value; // Match your claim name
            return int.TryParse(rank, out int val) && val >= 100;
        }
    }

    public record RoleDto(string Name, int Rank);
}