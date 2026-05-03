namespace Nexus.Backend.Models
{
    public class Role
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public int Rank { get; set; }

        public List<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

    }
}
