using Microsoft.EntityFrameworkCore;
using Nexus.Backend.Models;

namespace Nexus.Backend.Data
{
    public class NexusDbContext : DbContext
    {
        public NexusDbContext(DbContextOptions<NexusDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<Permission> Permissions { get; set; } = null!;
        public DbSet<RolePermission> RolePermissions { get; set; } = null!;
        public DbSet<Application> Applications { get; set; } = null!;
        public DbSet<UserPermissionOverride> UserPermissionOverrides { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Configure Composite Key for RolePermission (Junction Table)
            modelBuilder.Entity<RolePermission>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            // 2. Configure Permission Entity & Nullability
            // This allows User Overrides to have a NULL RoleId to avoid Index conflicts
            modelBuilder.Entity<Permission>()
                .Property(p => p.RoleId)
                .IsRequired(false);

            // 3. Configure Filtered Unique Indices
            // ROLE INDEX: One unique permission definition PER ROLE per APP
            modelBuilder.Entity<Permission>()
                .HasIndex(p => new { p.RoleId, p.AppName })
                .IsUnique()
                .HasFilter("[RoleId] IS NOT NULL");

            // USER INDEX: One unique permission definition PER USER per APP (for overrides)
            modelBuilder.Entity<Permission>()
                .HasIndex(p => new { p.UserId, p.AppName })
                .IsUnique()
                .HasFilter("[RoleId] IS NULL");

            // 4. Configure Relationships
            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId);

            modelBuilder.Entity<UserPermissionOverride>()
                .HasOne(uo => uo.User)
                .WithMany()
                .HasForeignKey(uo => uo.UserId);

            // 5. Seed Default Nexus Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin", Rank = 100 },
                new Role { Id = 2, Name = "SVP", Rank = 80 },
                new Role { Id = 3, Name = "VP", Rank = 60 },
                new Role { Id = 4, Name = "PM", Rank = 40 },
                new Role { Id = 5, Name = "TL", Rank = 20 },
                new Role { Id = 6, Name = "SE", Rank = 10 }
            );

            // 6. Seed Initial Applications
            modelBuilder.Entity<Application>().HasData(
                new Application
                {
                    Id = 1,
                    Name = "Identity Vault",
                    RoutePath = "/admin/users",
                    IconName = "Users",
                    IsActive = true,
                    IsInternal = true
                },
                new Application
                {
                    Id = 2,
                    Name = "App Registry",
                    RoutePath = "/admin/apps",
                    IconName = "Package",
                    IsActive = true,
                    IsInternal = true
                },
                new Application
                {
                    Id = 3,
                    Name = "Security Matrix",
                    RoutePath = "/admin/security",
                    IconName = "Shield",
                    IsActive = true,
                    IsInternal = true
                },
                new Application
                {
                    Id = 4,
                    Name = "Access Control",
                    RoutePath = "/admin/roles",
                    IconName = "GitGraph",
                    IsActive = true,
                    IsInternal = true
                },
                new Application
                {
                    Id = 5,
                    Name = "Role Permission Matrix",
                    RoutePath = "/admin/role-permissions",
                    IconName = "Permission",
                    IsActive = true,
                    IsInternal = true
                }
            );

            // 7. Seed Isolated Master Permissions (Admin Role Example)
            modelBuilder.Entity<Permission>().HasData(
                new Permission
                {
                    Id = 1,
                    Name = "ADMIN_VAULT_MASTER",
                    AppName = "Identity Vault",
                    RoleId = 1,
                    UserId = 0,
                    CanRead = true,
                    CanWrite = true,
                    CanEdit = true,
                    CanDelete = true,
                    Description = "Full access for Admin Role"
                },
                new Permission
                {
                    Id = 2,
                    Name = "ADMIN_DASHBOARD_MASTER",
                    AppName = "Dashboard",
                    RoleId = 1,
                    UserId = 0,
                    CanRead = true,
                    CanWrite = false,
                    CanEdit = false,
                    CanDelete = false,
                    Description = "Read-only access for Admin Role"
                }
            );
        }
    }
}