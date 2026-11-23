using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokeHub.Modules.Pokedex.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Add_Game_Dex_Entries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "game_dex_entries",
                schema: "pokedex",
                columns: table => new
                {
                    game_slug = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    species_id = table.Column<int>(type: "integer", nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_game_dex_entries", x => new { x.game_slug, x.species_id });
                    table.ForeignKey(
                        name: "fk_game_dex_entries_species_species_id",
                        column: x => x.species_id,
                        principalSchema: "pokedex",
                        principalTable: "species",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_game_dex_entries_species_id",
                schema: "pokedex",
                table: "game_dex_entries",
                column: "species_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "game_dex_entries",
                schema: "pokedex");
        }
    }
}
