namespace Nexus.Backend.Models
{
    public class Application
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string RoutePath { get; set; } // e.g., "/admin/users"
        public required string IconName { get; set; }  // Lucide icon name
        public bool IsActive { get; set; } = true;

        public bool IsInternal { get; set; }
    }
}
