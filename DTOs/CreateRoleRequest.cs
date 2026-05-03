namespace Nexus.Backend.DTOs
{
    public class CreateRoleRequest
    {
        public string RoleName { get; set; } = string.Empty;
        public int Rank { get; set; }
    }
}
