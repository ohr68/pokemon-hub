using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokeHub.Modules.Games.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Add_Game_Max_Species : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "max_species_id",
                schema: "games",
                table: "games",
                type: "integer",
                nullable: true);

            // FireRed has the first three generations.
            migrationBuilder.Sql("UPDATE games.games SET max_species_id = 386 WHERE slug = 'firered';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "max_species_id",
                schema: "games",
                table: "games");
        }
    }
}
