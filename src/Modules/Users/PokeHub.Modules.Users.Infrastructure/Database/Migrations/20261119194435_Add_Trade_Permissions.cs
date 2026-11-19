using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PokeHub.Modules.Users.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Add_Trade_Permissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "users",
                table: "permissions",
                column: "code",
                values: new object[]
                {
                    "trades:read",
                    "trades:update"
                });

            migrationBuilder.InsertData(
                schema: "users",
                table: "role_permissions",
                columns: new[] { "permission_code", "role_name" },
                values: new object[,]
                {
                    { "trades:read", "Administrator" },
                    { "trades:read", "Member" },
                    { "trades:update", "Administrator" },
                    { "trades:update", "Member" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "users",
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "trades:read", "Administrator" });

            migrationBuilder.DeleteData(
                schema: "users",
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "trades:read", "Member" });

            migrationBuilder.DeleteData(
                schema: "users",
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "trades:update", "Administrator" });

            migrationBuilder.DeleteData(
                schema: "users",
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "trades:update", "Member" });

            migrationBuilder.DeleteData(
                schema: "users",
                table: "permissions",
                keyColumn: "code",
                keyValue: "trades:read");

            migrationBuilder.DeleteData(
                schema: "users",
                table: "permissions",
                keyColumn: "code",
                keyValue: "trades:update");
        }
    }
}
