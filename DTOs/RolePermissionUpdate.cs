namespace Nexus.Backend.DTOs
{
    public class RolePermissionUpdate
    {
        public int RoleId { get; set; }
        public List<AppPermissionRequest> Permissions { get; set; }
    }

    public class AppPermissionRequest
    {
        public string AppName { get; set; }
        public bool CanRead { get; set; }
        public bool CanWrite { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}
