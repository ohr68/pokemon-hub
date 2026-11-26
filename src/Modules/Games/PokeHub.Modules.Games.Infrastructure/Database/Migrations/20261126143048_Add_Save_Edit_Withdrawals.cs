using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokeHub.Modules.Games.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Add_Save_Edit_Withdrawals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "withdrawn_catch_keys",
                schema: "games",
                table: "save_edits",
                type: "text[]",
                nullable: false,
                // Edits made before withdrawals existed took none out.
                defaultValueSql: "'{}'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "withdrawn_catch_keys",
                schema: "games",
                table: "save_edits");
        }
    }
}
