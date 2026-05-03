using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Nexus.Backend.Controllers
{
    [Authorize] // This ensures only logged-in users with a valid JWT can enter
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        [HttpGet("my-apps")]
        public IActionResult GetMyApps()
        {
            // Extract Rank from the JWT "ID Card"
            var rankClaim = User.Claims.FirstOrDefault(c => c.Type == "RoleRank")?.Value;

            if (string.IsNullOrEmpty(rankClaim))
            {
                // DEBUG: If it's still null, let's see what claims DID arrive
                var allClaims = User.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
                return BadRequest(new
                {
                    error = "Rank claim is missing",
                    receivedClaims = allClaims
                });
            }

            if (!int.TryParse(rankClaim, out int userRank)) return Unauthorized();

            // Define your App Library and their required ranks
            var allApps = new List<dynamic>
            {
                new { Name = "Internal Standup", Icon = "calendar", MinRank = 10, Url = "/standup" },
                new { Name = "Assessment Tool", Icon = "assignment", MinRank = 40, Url = "/assess" },
                new { Name = "Seating Arrangement", Icon = "map", MinRank = 20, Url = "/seating" },
                new { Name = "Role Manager", Icon = "security", MinRank = 80, Url = "/admin/roles" } // SVP and Admin only
            };

            // Filter apps based on user rank
            var visibleApps = allApps.Where(app => userRank >= app.MinRank).ToList();

            return Ok(new
            {
                User = User.Identity?.Name,
                Apps = visibleApps
            });
        }
    }
}