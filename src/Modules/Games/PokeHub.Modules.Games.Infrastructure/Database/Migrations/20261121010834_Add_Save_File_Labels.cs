using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokeHub.Modules.Games.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Add_Save_File_Labels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "label",
                schema: "games",
                table: "save_files",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "label",
                schema: "games",
                table: "save_files");
        }
    }
}
