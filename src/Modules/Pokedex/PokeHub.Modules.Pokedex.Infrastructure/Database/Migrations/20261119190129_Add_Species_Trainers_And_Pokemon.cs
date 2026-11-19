using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokeHub.Modules.Pokedex.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Add_Species_Trainers_And_Pokemon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "species",
                schema: "pokedex",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    generation = table.Column<int>(type: "integer", nullable: false),
                    primary_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    secondary_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    synced_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_species", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "trainers",
                schema: "pokedex",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    starter_species_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trainers", x => x.id);
                    table.ForeignKey(
                        name: "fk_trainers_species_starter_species_id",
                        column: x => x.starter_species_id,
                        principalSchema: "pokedex",
                        principalTable: "species",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "pokemon",
                schema: "pokedex",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    species_id = table.Column<int>(type: "integer", nullable: false),
                    trainer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    original_trainer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nickname = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    level = table.Column<int>(type: "integer", nullable: false),
                    is_shiny = table.Column<bool>(type: "boolean", nullable: false),
                    origin = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    obtained_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pokemon", x => x.id);
                    table.ForeignKey(
                        name: "fk_pokemon_species_species_id",
                        column: x => x.species_id,
                        principalSchema: "pokedex",
                        principalTable: "species",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_pokemon_trainers_original_trainer_id",
                        column: x => x.original_trainer_id,
                        principalSchema: "pokedex",
                        principalTable: "trainers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_pokemon_trainers_trainer_id",
                        column: x => x.trainer_id,
                        principalSchema: "pokedex",
                        principalTable: "trainers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ownership_history",
                schema: "pokedex",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pokemon_id = table.Column<Guid>(type: "uuid", nullable: false),
                    trainer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    species_id = table.Column<int>(type: "integer", nullable: false),
                    change = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    occurred_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ownership_history", x => x.id);
                    table.ForeignKey(
                        name: "fk_ownership_history_pokemon_pokemon_id",
                        column: x => x.pokemon_id,
                        principalSchema: "pokedex",
                        principalTable: "pokemon",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_ownership_history_species_species_id",
                        column: x => x.species_id,
                        principalSchema: "pokedex",
                        principalTable: "species",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_ownership_history_trainers_trainer_id",
                        column: x => x.trainer_id,
                        principalSchema: "pokedex",
                        principalTable: "trainers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_ownership_history_pokemon_id",
                schema: "pokedex",
                table: "ownership_history",
                column: "pokemon_id");

            migrationBuilder.CreateIndex(
                name: "ix_ownership_history_species_id",
                schema: "pokedex",
                table: "ownership_history",
                column: "species_id");

            migrationBuilder.CreateIndex(
                name: "ix_ownership_history_trainer_id_change_species_id",
                schema: "pokedex",
                table: "ownership_history",
                columns: new[] { "trainer_id", "change", "species_id" });

            migrationBuilder.CreateIndex(
                name: "ix_pokemon_original_trainer_id",
                schema: "pokedex",
                table: "pokemon",
                column: "original_trainer_id");

            migrationBuilder.CreateIndex(
                name: "ix_pokemon_species_id",
                schema: "pokedex",
                table: "pokemon",
                column: "species_id");

            migrationBuilder.CreateIndex(
                name: "ix_pokemon_trainer_id_status",
                schema: "pokedex",
                table: "pokemon",
                columns: new[] { "trainer_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_species_name",
                schema: "pokedex",
                table: "species",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_trainers_starter_species_id",
                schema: "pokedex",
                table: "trainers",
                column: "starter_species_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ownership_history",
                schema: "pokedex");

            migrationBuilder.DropTable(
                name: "pokemon",
                schema: "pokedex");

            migrationBuilder.DropTable(
                name: "trainers",
                schema: "pokedex");

            migrationBuilder.DropTable(
                name: "species",
                schema: "pokedex");
        }
    }
}
