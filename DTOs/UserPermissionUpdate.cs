namespace Nexus.Backend.DTOs
{
    public class UserPermissionUpdate
    {
        public int UserId { get; set; } // The ID of the specific user (e.g., User 3)
        public List<UserAppPermissionRequest> Permissions { get; set; }
    }

    public class UserAppPermissionRequest
    {
        public string AppName { get; set; }
        public bool CanRead { get; set; }
        public bool CanWrite { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}
