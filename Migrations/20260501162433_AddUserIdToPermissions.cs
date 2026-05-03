using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Nexus.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.AlterColumn<string>(
                name: "AppName",
                table: "Permissions",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Permissions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_UserId_AppName",
                table: "Permissions",
                columns: new[] { "UserId", "AppName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Permissions_UserId_AppName",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Permissions");

            migrationBuilder.AlterColumn<string>(
                name: "AppName",
                table: "Permissions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "AppName", "CanDelete", "CanEdit", "CanRead", "CanWrite", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Identity Vault", false, false, false, false, "Full access to manage corporate agents", "USER_MANAGEMENT_FULL" },
                    { 2, "Dashboard", false, false, false, false, "Permission to view the main analytics dashboard", "DASHBOARD_VIEW_BASIC" }
                });
        }
    }
}
