using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexus.Backend.Data;
using Nexus.Backend.DTOs;
using Nexus.Backend.Models;

namespace Nexus.Backend.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SecurityController : ControllerBase
    {
        private readonly NexusDbContext _context;
        public SecurityController(NexusDbContext context) => _context = context;

        [HttpGet("user-matrix/{userId}")]
        public async Task<IActionResult> GetUserMatrix(
            int userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 6,
            [FromQuery] string search = "")
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound("User not found.");

            var appQuery = _context.Applications.Where(a=>a.IsInternal ==false).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                appQuery = appQuery.Where(a => a.Name.Contains(search));
            }

            var totalCount = await appQuery.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var pagedApps = await appQuery
                .OrderBy(a => a.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // ✅ CHANGED: Fetch permissions directly from the Permissions table using UserId
            var userSpecificPermissions = await _context.Permissions
                .Where(p => p.UserId == userId)
                .ToListAsync();

            var matrix = pagedApps.Select(app =>
            {
                // ✅ CHANGED: Look for the record in the Permissions table
                var custom = userSpecificPermissions.FirstOrDefault(p => p.AppName == app.Name);

                var roleDefault = user.Role.RolePermissions
                    .FirstOrDefault(rp => rp.Permission.AppName == app.Name)?.Permission;

                // Priority: User-specific Row > Role Default
                var activePerm = custom ?? roleDefault;

                return new
                {
                    appName = app.Name,
                    canRead = activePerm?.CanRead ?? false,
                    canWrite = activePerm?.CanWrite ?? false,
                    canEdit = activePerm?.CanEdit ?? false,
                    canDelete = activePerm?.CanDelete ?? false
                };
            }).ToList();

            return Ok(new
            {
                items = matrix,
                totalCount = totalCount,
                totalPages = totalPages,
                pageNumber = page,
                pageSize = pageSize
            });
        }

        [HttpPost("update-user-matrix")]
        public async Task<IActionResult> UpdateUserMatrix([FromBody] UserPermissionUpdate req)
        {
            // Validate that we have a real User ID
            if (req.UserId <= 0) return BadRequest("Invalid User ID for override.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in req.Permissions)
                {
                    // Look for an existing OVERRIDE (where RoleId is NULL)
                    var permission = await _context.Permissions.FirstOrDefaultAsync(p =>
                        p.UserId == req.UserId &&
                        p.AppName == item.AppName && p.RoleId == null);

                    if (permission != null)
                    {
                        // Update existing user override
                        permission.CanRead = item.CanRead;
                        permission.CanWrite = item.CanWrite;
                        permission.CanEdit = item.CanEdit;
                        permission.CanDelete = item.CanDelete;
                        _context.Permissions.Update(permission);
                    }
                    else
                    {
                        // Create a NEW user override record
                        _context.Permissions.Add(new Permission
                        {
                            Name = $"{item.AppName.ToUpper()}_USER_{req.UserId}_OVERRIDE",
                            AppName = item.AppName,
                            UserId = req.UserId,
                            RoleId = null, // Explicitly mark as user-specific
                            CanRead = item.CanRead,
                            CanWrite = item.CanWrite,
                            CanEdit = item.CanEdit,
                            CanDelete = item.CanDelete,
                            Description = $"User-specific override for User ID {req.UserId} on {item.AppName}"
                        });
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(new { message = "User overrides saved successfully." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Return the actual DB error to help debugging
                return StatusCode(500, new { message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpGet("assigned-apps/{userId}")]
        public async Task<IActionResult> GetAssignedApps(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound("User not found.");

            // ✅ CHANGED: Fetch permissions directly from the Permissions table
            var userSpecificPermissions = await _context.Permissions
                .Where(p => p.UserId == userId)
                .ToListAsync();

            var allApps = await _context.Applications.ToListAsync();

            var assignedApps = allApps.Select(app =>
            {
                // ✅ CHANGED: Logic to use the new table structure
                var custom = userSpecificPermissions.FirstOrDefault(p => p.AppName == app.Name);

                var roleDefault = user.Role.RolePermissions
                    .FirstOrDefault(rp => rp.Permission.AppName == app.Name)?.Permission;

                var activePerm = custom ?? roleDefault;

                if (activePerm == null || !activePerm.CanRead) return null;

                return new
                {
                    app.Id,
                    app.Name,
                    app.RoutePath,
                    app.IconName,
                    app.IsActive
                };
            })
            .Where(app => app != null)
            .ToList();

            return Ok(assignedApps);
        }

        [HttpPost("update-role-permissions")]
        public async Task<IActionResult> UpdateRolePermissions([FromBody] RolePermissionUpdate req)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in req.Permissions)
                {
                    // STEP 1: Handle the Permission Definition (Isolated by RoleId)
                    var permission = await _context.Permissions.FirstOrDefaultAsync(p =>
                        p.RoleId == req.RoleId && p.AppName == item.AppName);

                    if (permission != null)
                    {
                        // Update existing record for this role
                        permission.CanRead = item.CanRead;
                        permission.CanWrite = item.CanWrite;
                        permission.CanEdit = item.CanEdit;
                        permission.CanDelete = item.CanDelete;
                        _context.Permissions.Update(permission);
                    }
                    else
                    {
                        // Create brand new isolated record for this role
                        permission = new Permission
                        {
                            Name = $"{item.AppName.ToUpper()}_ROLE_{req.RoleId}",
                            AppName = item.AppName,
                            RoleId = req.RoleId, // New Column
                            UserId = 0,
                            CanRead = item.CanRead,
                            CanWrite = item.CanWrite,
                            CanEdit = item.CanEdit,
                            CanDelete = item.CanDelete,
                            Description = $"Role-based permissions for Role ID {req.RoleId} on {item.AppName}"
                        };
                        _context.Permissions.Add(permission);
                    }

                    // Must save to generate/update the Permission.Id for the bridge
                    await _context.SaveChangesAsync();

                    // STEP 2: Synchronize the RolePermission Bridge
                    var bridgeLink = await _context.RolePermissions
                        .FirstOrDefaultAsync(rp => rp.RoleId == req.RoleId &&
                            _context.Permissions.Any(p => p.Id == rp.PermissionId && p.AppName == item.AppName));

                    if (bridgeLink != null)
                    {
                        if (bridgeLink.PermissionId != permission.Id)
                        {
                            _context.RolePermissions.Remove(bridgeLink);
                            await _context.SaveChangesAsync(); // Clear tracker for composite key

                            _context.RolePermissions.Add(new RolePermission
                            {
                                RoleId = req.RoleId,
                                PermissionId = permission.Id,
                                PermissionName = $"{item.AppName.ToUpper()}_ROLE_{req.RoleId}"
                            });
                        }
                    }
                    else
                    {
                        _context.RolePermissions.Add(new RolePermission
                        {
                            RoleId = req.RoleId,
                            PermissionId = permission.Id,
                            PermissionName = $"{item.AppName.ToUpper()}_ROLE_{req.RoleId}"
                        });
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(new { message = "Role-based permissions and bridge synchronized successfully." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Sync Error: " + ex.Message });
            }
        }

        [HttpGet("role-matrix/{roleId}")]
        public async Task<IActionResult> GetRoleMatrix(int roleId)
        {
            try
            {
                // 1. Fetch the Role and its current linked permissions
                var role = await _context.Roles
                    .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                    .FirstOrDefaultAsync(r => r.Id == roleId);

                if (role == null) return NotFound("Role not found.");

                // 2. Fetch all registered applications from the App Registry
                var allApps = await _context.Applications.Where(a => a.IsInternal).ToListAsync();

                // 3. Build the matrix by mapping apps to existing role permissions
                var matrix = allApps.Select(app =>
                {
                    var existingPerm = role.RolePermissions
                        .FirstOrDefault(rp => rp.Permission.AppName == app.Name)?.Permission;

                    return new
                    {
                        appName = app.Name,
                        canRead = existingPerm?.CanRead ?? false,
                        canWrite = existingPerm?.CanWrite ?? false,
                        canEdit = existingPerm?.CanEdit ?? false,
                        canDelete = existingPerm?.CanDelete ?? false
                    };
                }).ToList();

                return Ok(new { items = matrix });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving role matrix: " + ex.Message });
            }
        }

    }
    public class RoleMatrixUpdate
    {
        public int RoleId { get; set; }
        public List<MatrixItem> Permissions { get; set; }
    }
    public class BulkMatrixUpdate
    {
        public int UserId { get; set; }
        public List<MatrixItem> Permissions { get; set; }
    }

    public class MatrixItem
    {
        public string AppName { get; set; }
        public bool CanRead { get; set; }
        public bool CanWrite { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}