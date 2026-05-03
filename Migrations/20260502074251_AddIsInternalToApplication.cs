using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Nexus.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddIsInternalToApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsInternal",
                table: "Applications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Applications",
                keyColumn: "Id",
                keyValue: 1,
                column: "IsInternal",
                value: true);

            migrationBuilder.UpdateData(
                table: "Applications",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "IconName", "IsInternal", "Name", "RoutePath" },
                values: new object[] { "Package", true, "App Registry", "/admin/apps" });

            migrationBuilder.InsertData(
                table: "Applications",
                columns: new[] { "Id", "IconName", "IsActive", "IsInternal", "Name", "RoutePath" },
                values: new object[,]
                {
                    { 3, "Shield", true, true, "Security Matrix", "/admin/security" },
                    { 4, "GitGraph", true, true, "Access Control", "/admin/roles" },
                    { 5, "Permission", true, true, "Role Permission Matrix", "/admin/role-permissions" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Applications",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Applications",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Applications",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DropColumn(
                name: "IsInternal",
                table: "Applications");

            migrationBuilder.UpdateData(
                table: "Applications",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "IconName", "Name", "RoutePath" },
                values: new object[] { "LayoutDashboard", "Dashboard", "/dashboard" });
        }
    }
}
