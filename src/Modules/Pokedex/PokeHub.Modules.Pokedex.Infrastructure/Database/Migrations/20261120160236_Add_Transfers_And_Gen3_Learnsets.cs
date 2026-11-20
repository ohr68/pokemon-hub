using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokeHub.Modules.Pokedex.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Add_Transfers_And_Gen3_Learnsets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "caught_in_game_id",
                schema: "pokedex",
                table: "pokemon",
                newName: "catch_key_game_id");

            migrationBuilder.RenameIndex(
                name: "ix_pokemon_caught_in_game_id_catch_key",
                schema: "pokedex",
                table: "pokemon",
                newName: "ix_pokemon_catch_key_game_id_catch_key");

            migrationBuilder.AddColumn<bool>(
                name: "awaiting_delivery",
                schema: "pokedex",
                table: "pokemon",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // Pokémon moved into a save before deliveries existed never reached the game's own
            // save (only those with a catch key are in there): they are waiting for delivery.
            migrationBuilder.Sql(
                """
                UPDATE pokedex.pokemon
                SET awaiting_delivery = true
                WHERE save_id IS NOT NULL AND catch_key IS NULL AND status = 'Active';
                """);

            migrationBuilder.AddColumn<byte[]>(
                name: "game_data",
                schema: "pokedex",
                table: "pokemon",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "game_data_format",
                schema: "pokedex",
                table: "pokemon",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "moves",
                schema: "pokedex",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    pp = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_moves", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "species_gen3_level_up_moves",
                schema: "pokedex",
                columns: table => new
                {
                    order = table.Column<int>(type: "integer", nullable: false),
                    species_id = table.Column<int>(type: "integer", nullable: false),
                    level = table.Column<int>(type: "integer", nullable: false),
                    move_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_species_gen3_level_up_moves", x => new { x.species_id, x.order });
                    table.ForeignKey(
                        name: "fk_species_gen3_level_up_moves_move_move_id",
                        column: x => x.move_id,
                        principalSchema: "pokedex",
                        principalTable: "moves",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_species_gen3_level_up_moves_species_species_id",
                        column: x => x.species_id,
                        principalSchema: "pokedex",
                        principalTable: "species",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_species_gen3_level_up_moves_move_id",
                schema: "pokedex",
                table: "species_gen3_level_up_moves",
                column: "move_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "species_gen3_level_up_moves",
                schema: "pokedex");

            migrationBuilder.DropTable(
                name: "moves",
                schema: "pokedex");

            migrationBuilder.DropColumn(
                name: "awaiting_delivery",
                schema: "pokedex",
                table: "pokemon");

            migrationBuilder.DropColumn(
                name: "game_data",
                schema: "pokedex",
                table: "pokemon");

            migrationBuilder.DropColumn(
                name: "game_data_format",
                schema: "pokedex",
                table: "pokemon");

            migrationBuilder.RenameColumn(
                name: "catch_key_game_id",
                schema: "pokedex",
                table: "pokemon",
                newName: "caught_in_game_id");

            migrationBuilder.RenameIndex(
                name: "ix_pokemon_catch_key_game_id_catch_key",
                schema: "pokedex",
                table: "pokemon",
                newName: "ix_pokemon_caught_in_game_id_catch_key");
        }
    }
}
