using Nexus.Backend.Data;
using Nexus.Backend.Models;

namespace Nexus.Backend.Services
{
    public class RoleService
    {
        private readonly NexusDbContext _context;
        public RoleService(NexusDbContext context)
        {
            _context = context;
        }

        public async Task<String>CreateSubRole(int creatorRank, string newRoleName, int newRoleRank)
        {
            if(newRoleRank >= creatorRank)
                            {
                return "Error: You cannot create a role with equal or higher rank than yourself.";
            }
            var newRole = new Role
            {
                Name = newRoleName,
                Rank = newRoleRank
            };
            _context.Roles.Add(newRole);
            await _context.SaveChangesAsync();
            return $"Success: Role '{newRoleName}' created with rank {newRoleRank}.";
        }
    }
}
