using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokeHub.Modules.Pokedex.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Add_Saves_And_Locations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "catch_key",
                schema: "pokedex",
                table: "pokemon",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "caught_in_game_id",
                schema: "pokedex",
                table: "pokemon",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "save_id",
                schema: "pokedex",
                table: "pokemon",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "save_id",
                schema: "pokedex",
                table: "ownership_history",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "saves",
                schema: "pokedex",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    trainer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    game_id = table.Column<Guid>(type: "uuid", nullable: false),
                    game_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_saves", x => x.id);
                    table.ForeignKey(
                        name: "fk_saves_trainers_trainer_id",
                        column: x => x.trainer_id,
                        principalSchema: "pokedex",
                        principalTable: "trainers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_pokemon_caught_in_game_id_catch_key",
                schema: "pokedex",
                table: "pokemon",
                columns: new[] { "caught_in_game_id", "catch_key" },
                unique: true,
                filter: "catch_key IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_pokemon_save_id",
                schema: "pokedex",
                table: "pokemon",
                column: "save_id");

            migrationBuilder.CreateIndex(
                name: "ix_saves_trainer_id_game_id",
                schema: "pokedex",
                table: "saves",
                columns: new[] { "trainer_id", "game_id" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_pokemon_saves_save_id",
                schema: "pokedex",
                table: "pokemon",
                column: "save_id",
                principalSchema: "pokedex",
                principalTable: "saves",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_pokemon_saves_save_id",
                schema: "pokedex",
                table: "pokemon");

            migrationBuilder.DropTable(
                name: "saves",
                schema: "pokedex");

            migrationBuilder.DropIndex(
                name: "ix_pokemon_caught_in_game_id_catch_key",
                schema: "pokedex",
                table: "pokemon");

            migrationBuilder.DropIndex(
                name: "ix_pokemon_save_id",
                schema: "pokedex",
                table: "pokemon");

            migrationBuilder.DropColumn(
                name: "catch_key",
                schema: "pokedex",
                table: "pokemon");

            migrationBuilder.DropColumn(
                name: "caught_in_game_id",
                schema: "pokedex",
                table: "pokemon");

            migrationBuilder.DropColumn(
                name: "save_id",
                schema: "pokedex",
                table: "pokemon");

            migrationBuilder.DropColumn(
                name: "save_id",
                schema: "pokedex",
                table: "ownership_history");
        }
    }
}
