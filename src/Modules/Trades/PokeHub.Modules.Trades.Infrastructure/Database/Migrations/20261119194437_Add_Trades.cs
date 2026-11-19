using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokeHub.Modules.Trades.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Add_Trades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "trades",
                schema: "trades",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    proposer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    proposer_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    recipient_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recipient_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    offered_pokemon_id = table.Column<Guid>(type: "uuid", nullable: false),
                    offered_species_id = table.Column<int>(type: "integer", nullable: false),
                    offered_species_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    offered_nickname = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    offered_level = table.Column<int>(type: "integer", nullable: false),
                    offered_is_shiny = table.Column<bool>(type: "boolean", nullable: false),
                    requested_pokemon_id = table.Column<Guid>(type: "uuid", nullable: false),
                    requested_species_id = table.Column<int>(type: "integer", nullable: false),
                    requested_species_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    requested_nickname = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    requested_level = table.Column<int>(type: "integer", nullable: false),
                    requested_is_shiny = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    resolved_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    failure_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trades", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_trades_proposer_id_created_on_utc",
                schema: "trades",
                table: "trades",
                columns: new[] { "proposer_id", "created_on_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_trades_recipient_id_created_on_utc",
                schema: "trades",
                table: "trades",
                columns: new[] { "recipient_id", "created_on_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trades",
                schema: "trades");
        }
    }
}
