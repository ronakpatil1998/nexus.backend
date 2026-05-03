namespace Nexus.Backend.Models
{
    public class Permission
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public required string Name { get; set; }
        public required string AppName { get; set; }
        public required string Description { get; set; }

        public int? RoleId { get; set; }

        public bool CanRead { get; set; }
        public bool CanWrite { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }

        public List<RolePermission> RolePermissions { get; set; } = new();

        public string PermissionKey =>
            $"{AppName}:{(CanRead ? "R" : "")}{(CanWrite ? "W" : "")}{(CanEdit ? "E" : "")}{(CanDelete ? "D" : "")}";
    }
}
