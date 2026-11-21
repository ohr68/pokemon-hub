using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokeHub.Modules.Progression.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Add_Trophies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "catches",
                schema: "progression",
                table: "trainer_progress",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "evolutions",
                schema: "progression",
                table: "trainer_progress",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "shiny_catches",
                schema: "progression",
                table: "trainer_progress",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "species_discovered",
                schema: "progression",
                table: "trainer_progress",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "starters_chosen",
                schema: "progression",
                table: "trainer_progress",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "trades",
                schema: "progression",
                table: "trainer_progress",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Trainers who earned XP before trophies: count from their XP ledger.
            migrationBuilder.Sql(
                """
                UPDATE progression.trainer_progress p SET
                    starters_chosen = (SELECT count(*) FROM progression.xp_entries e WHERE e.trainer_id = p.trainer_id AND e.reason = 'ChoseStarter'),
                    catches = (SELECT count(*) FROM progression.xp_entries e WHERE e.trainer_id = p.trainer_id AND e.reason = 'CaughtPokemon'),
                    shiny_catches = (SELECT count(*) FROM progression.xp_entries e WHERE e.trainer_id = p.trainer_id AND e.reason = 'CaughtShiny'),
                    species_discovered = (SELECT count(*) FROM progression.xp_entries e WHERE e.trainer_id = p.trainer_id AND e.reason = 'NewSpecies'),
                    trades = (SELECT count(*) FROM progression.xp_entries e WHERE e.trainer_id = p.trainer_id AND e.reason = 'Traded'),
                    evolutions = (SELECT count(*) FROM progression.xp_entries e WHERE e.trainer_id = p.trainer_id AND e.reason = 'Evolved');
                """);

            migrationBuilder.CreateTable(
                name: "trainer_trophies",
                schema: "progression",
                columns: table => new
                {
                    trainer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    trophy_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    earned_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trainer_trophies", x => new { x.trainer_id, x.trophy_code });
                    table.ForeignKey(
                        name: "fk_trainer_trophies_trainer_progress_trainer_id",
                        column: x => x.trainer_id,
                        principalSchema: "progression",
                        principalTable: "trainer_progress",
                        principalColumn: "trainer_id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trainer_trophies",
                schema: "progression");

            migrationBuilder.DropColumn(
                name: "catches",
                schema: "progression",
                table: "trainer_progress");

            migrationBuilder.DropColumn(
                name: "evolutions",
                schema: "progression",
                table: "trainer_progress");

            migrationBuilder.DropColumn(
                name: "shiny_catches",
                schema: "progression",
                table: "trainer_progress");

            migrationBuilder.DropColumn(
                name: "species_discovered",
                schema: "progression",
                table: "trainer_progress");

            migrationBuilder.DropColumn(
                name: "starters_chosen",
                schema: "progression",
                table: "trainer_progress");

            migrationBuilder.DropColumn(
                name: "trades",
                schema: "progression",
                table: "trainer_progress");
        }
    }
}
