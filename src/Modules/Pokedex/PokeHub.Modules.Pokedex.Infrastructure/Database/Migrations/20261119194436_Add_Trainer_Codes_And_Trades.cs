using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokeHub.Modules.Pokedex.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Add_Trainer_Codes_And_Trades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "code",
                schema: "pokedex",
                table: "trainers",
                type: "character varying(9)",
                maxLength: 9,
                nullable: false,
                defaultValue: "");

            // Existing trainers get their own code before the unique index is created. The
            // subquery refers to the row so PostgreSQL draws new random characters for each one.
            migrationBuilder.Sql(
                """
                UPDATE pokedex.trainers t
                SET code = (
                    SELECT string_agg(
                        CASE WHEN i = 5 THEN '-'
                             ELSE substr('ABCDEFGHJKLMNPQRSTUVWXYZ23456789', 1 + floor(random() * 32)::int, 1)
                        END,
                        '' ORDER BY i)
                    FROM generate_series(1, 9) AS i
                    WHERE t.id IS NOT NULL)
                WHERE code = '';
                """);

            migrationBuilder.AddColumn<int>(
                name: "trade_evolution_species_id",
                schema: "pokedex",
                table: "species",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "trade_id",
                schema: "pokedex",
                table: "ownership_history",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_trainers_code",
                schema: "pokedex",
                table: "trainers",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_ownership_history_trade_id",
                schema: "pokedex",
                table: "ownership_history",
                column: "trade_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_trainers_code",
                schema: "pokedex",
                table: "trainers");

            migrationBuilder.DropIndex(
                name: "ix_ownership_history_trade_id",
                schema: "pokedex",
                table: "ownership_history");

            migrationBuilder.DropColumn(
                name: "code",
                schema: "pokedex",
                table: "trainers");

            migrationBuilder.DropColumn(
                name: "trade_evolution_species_id",
                schema: "pokedex",
                table: "species");

            migrationBuilder.DropColumn(
                name: "trade_id",
                schema: "pokedex",
                table: "ownership_history");
        }
    }
}
