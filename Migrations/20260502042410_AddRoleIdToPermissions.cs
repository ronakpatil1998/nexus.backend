using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Nexus.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleIdToPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Permissions_UserId_AppName",
                table: "Permissions");

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 2, 1 });

            migrationBuilder.AddColumn<int>(
                name: "RoleId",
                table: "Permissions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "AppName", "CanDelete", "CanEdit", "CanRead", "CanWrite", "Description", "Name", "RoleId", "UserId" },
                values: new object[,]
                {
                    { 1, "Identity Vault", true, true, true, true, "", "ADMIN_VAULT_MASTER", 1, 0 },
                    { 2, "Dashboard", false, false, true, false, "", "ADMIN_DASHBOARD_MASTER", 1, 0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_RoleId_AppName",
                table: "Permissions",
                columns: new[] { "RoleId", "AppName" },
                unique: true,
                filter: "[RoleId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_UserId_AppName",
                table: "Permissions",
                columns: new[] { "UserId", "AppName" },
                unique: true,
                filter: "[RoleId] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Permissions_RoleId_AppName",
                table: "Permissions");

            migrationBuilder.DropIndex(
                name: "IX_Permissions_UserId_AppName",
                table: "Permissions");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "Permissions");

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId", "PermissionName" },
                values: new object[,]
                {
                    { 1, 1, "IDENTITY_VAULT_FULL_ACCESS" },
                    { 2, 1, "DASHBOARD_VIEW" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_UserId_AppName",
                table: "Permissions",
                columns: new[] { "UserId", "AppName" },
                unique: true);
        }
    }
}
