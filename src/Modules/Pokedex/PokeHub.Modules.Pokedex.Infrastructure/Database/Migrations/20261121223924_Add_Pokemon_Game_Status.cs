using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokeHub.Modules.Pokedex.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Add_Pokemon_Game_Status : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pokemon_game_status",
                schema: "pokedex",
                columns: table => new
                {
                    pokemon_id = table.Column<Guid>(type: "uuid", nullable: false),
                    game_id = table.Column<Guid>(type: "uuid", nullable: false),
                    level = table.Column<int>(type: "integer", nullable: false),
                    battle_data = table.Column<string>(type: "jsonb", nullable: false),
                    updated_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pokemon_game_status", x => new { x.pokemon_id, x.game_id });
                    table.ForeignKey(
                        name: "fk_pokemon_game_status_pokemon_pokemon_id",
                        column: x => x.pokemon_id,
                        principalSchema: "pokedex",
                        principalTable: "pokemon",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Keeps what the last syncs reported, under the game the Pokémon is in.
            migrationBuilder.Sql(
                """
                INSERT INTO pokedex.pokemon_game_status (pokemon_id, game_id, level, battle_data, updated_on_utc)
                SELECT p.id, sv.game_id, p.level, p.battle_data, COALESCE(p.battle_data_updated_on_utc, now())
                FROM pokedex.pokemon p
                JOIN pokedex.saves sv ON sv.id = p.save_id
                WHERE p.battle_data IS NOT NULL;
                """);

            migrationBuilder.DropColumn(
                name: "battle_data",
                schema: "pokedex",
                table: "pokemon");

            migrationBuilder.DropColumn(
                name: "battle_data_updated_on_utc",
                schema: "pokedex",
                table: "pokemon");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pokemon_game_status",
                schema: "pokedex");

            migrationBuilder.AddColumn<string>(
                name: "battle_data",
                schema: "pokedex",
                table: "pokemon",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "battle_data_updated_on_utc",
                schema: "pokedex",
                table: "pokemon",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
