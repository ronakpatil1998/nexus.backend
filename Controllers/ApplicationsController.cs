using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexus.Backend.Data;
using Nexus.Backend.Models;

namespace Nexus.Backend.Controllers
{
    [Authorize] // Only authenticated users (Admins) can register apps
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationsController : ControllerBase
    {
        private readonly NexusDbContext _context;
        public ApplicationsController(NexusDbContext context) => _context = context;

        // 1. Register a new System App
        [HttpPost("register")]
        public async Task<IActionResult> RegisterApp([FromBody] Application app)
        {
            if (await _context.Applications.AnyAsync(a => a.Name == app.Name))
                return BadRequest("Application already exists in the Nexus ecosystem.");

            _context.Applications.Add(app);
            await _context.SaveChangesAsync();
            return Ok(app);
        }

        // 2. Get list of all apps for the Security Matrix
        [HttpGet("list")]
        public async Task<IActionResult> GetApplications(int page = 1, int pageSize = 10, string search = "")
        {
            var query = _context.Applications.AsQueryable();

            // 1. Filter by Search Term
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(a => a.Name.Contains(search) || a.RoutePath.Contains(search));
            }

            // 2. Total Count for Pagination UI
            var totalCount = await query.CountAsync();

            // 3. Paginate the Data
            var items = await query
                .OrderBy(a => a.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                items = items,
                totalCount = totalCount,
                pageNumber = page,
                pageSize = pageSize,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateApp(int id, [FromBody] Application updatedApp)
        {
            var app = await _context.Applications.FindAsync(id);
            if (app == null) return NotFound("Application not found.");

            // Update fields
            app.Name = updatedApp.Name;
            app.RoutePath = updatedApp.RoutePath;
            app.IconName = updatedApp.IconName;
            app.IsActive = updatedApp.IsActive;

            await _context.SaveChangesAsync();
            return Ok(app);
        }

        // 2. DELETE APP (Purge)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteApp(int id)
        {
            var app = await _context.Applications.FindAsync(id);
            if (app == null) return NotFound("Application not found.");

            // Optional: Also clear any permissions linked to this app name
            var relatedPermissions = _context.Permissions.Where(p => p.AppName == app.Name);
            _context.Permissions.RemoveRange(relatedPermissions);

            _context.Applications.Remove(app);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Application purged from ecosystem." });
        }
    }
}