namespace Nexus.Backend.Models
{
    public class RolePermission
    {
        public int PermissionId { get; set; }
        public int RoleId { get; set; }

        public Role? Role { get; set; }
        public Permission? Permission { get; set; }
        public required string PermissionName { get; set; } // e.g., "VIEW_DASHBOARD", "MANAGE_USERS"
    }
}
