using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Nexus.Backend.Data;
using Nexus.Backend.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using RegisterRequest = Nexus.Backend.Models.RegisterRequest;

namespace Nexus.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly NexusDbContext ?_context;
        private readonly IConfiguration ?_config;

        public AuthController(NexusDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [Authorize] // 🔒 No one can call this without a token
        [HttpPost("admin/create-user")]
        public async Task<IActionResult> AdminCreateUser([FromBody] RegisterRequest request)
        {
            try
            {
                // 1. Validate Input Model
                if (request == null || string.IsNullOrWhiteSpace(request.Email))
                {
                    return BadRequest("Invalid user data provided.");
                }

                // 2. Security Check: Extract and Validate Rank
                var creatorRankClaim = User.Claims.FirstOrDefault(c => c.Type == "RoleRank")?.Value;

                if (string.IsNullOrEmpty(creatorRankClaim))
                {
                    return Unauthorized("Your token is valid, but the 'Rank' claim is missing. Please re-login.");
                }

                if (!int.TryParse(creatorRankClaim, out int rankValue) || rankValue < 100)
                {
                    return StatusCode(403, "Access Denied: Only System Admins (Rank 100) can provision users.");
                }

                // 3. Database Check: Avoid Duplicates
                bool userExists = await _context.Users.AnyAsync(u => u.Email.ToLower() == request.Email.ToLower());
                if (userExists)
                {
                    return Conflict($"A user with the email '{request.Email}' is already registered.");
                }

                // 4. Create and Save User
                var newUser = new User
                {
                    FullName = request.FullName,
                    Email = request.Email,
                    PasswordHash = request.Password, // Note: Use BCrypt or Identity PasswordHasher in production
                    RoleId = request.RoleId
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(AdminCreateUser), new { id = newUser.Id }, new
                {
                    message = "User provisioned successfully",
                    user = newUser.Email,
                    assignedRole = newUser.RoleId
                });
            }
            catch (DbUpdateException dbEx)
            {
                // Occurs if there's a SQL constraint violation or connection issue
                return StatusCode(500, $"Database Error: {dbEx.InnerException?.Message ?? dbEx.Message}");
            }
            catch (Exception ex)
            {
                // The "Catch-All" to prevent a generic 500
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // 1. Fetch User with Role and Role-based Permissions
            var user = await _context.Users
                .Include(u => u.Role)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.PasswordHash == request.Password);

            if (user == null) return Unauthorized("Invalid credentials.");

            // 2. Fetch User-Specific Overrides
            var userOverrides = await _context.UserPermissionOverrides
                .Where(uo => uo.UserId == user.Id)
                .Include(uo => uo.Permission)
                .Select(uo => uo.Permission)
                .ToListAsync();

            // 3. The Nexus Merge & Negative Filter Logic
            var rolePermissions = user.Role.RolePermissions.Select(rp => rp.Permission).ToList();

            // We replace Role defaults with Overrides, then filter out any app where ALL flags are false
            var mergedPermissions = rolePermissions
                .Where(rp => !userOverrides.Any(uo => uo.AppName == rp.AppName))
                .Concat(userOverrides)
                // --- THE FIX: Filter out apps with zero access ---
                .Where(p => p.CanRead || p.CanWrite || p.CanEdit || p.CanDelete)
                .ToList();

            // 4. Generate Matrix Strings for Frontend
            var pStrings = mergedPermissions.Select(p =>
                $"{p.AppName}:{(p.CanRead ? "R" : "")}{(p.CanWrite ? "W" : "")}{(p.CanEdit ? "E" : "")}{(p.CanDelete ? "D" : "")}"
            ).ToList();

            // 5. Sidebar Sync: Only pull Apps that have at least one permission flag enabled
            var activeAppNames = mergedPermissions.Select(p => p.AppName).Distinct().ToList();
            var assignedApps = await _context.Applications
                .Where(a => activeAppNames.Contains(a.Name))
                .ToListAsync();

            // 6. Build Claims
            var claims = new List<Claim> {
        new Claim(ClaimTypes.Name, user.FullName),
        new Claim("Rank", user.Role.Rank.ToString()),
        new Claim("UserId", user.Id.ToString())
    };

            foreach (var pStr in pStrings) claims.Add(new Claim("Permission", pStr));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(8),
                signingCredentials: creds
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                user = new
                {
                    fullName = user.FullName,
                    rank = user.Role.Rank,
                    permissions = pStrings,
                    assignedApps = assignedApps // Ronak won't see restricted apps here anymore
                }
            });
        }
    }

    public record LoginRequest(string Email, string Password);
}
