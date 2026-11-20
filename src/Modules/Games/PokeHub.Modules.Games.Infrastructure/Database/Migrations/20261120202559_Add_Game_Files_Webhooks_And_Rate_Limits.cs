using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokeHub.Modules.Games.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Add_Game_Files_Webhooks_And_Rate_Limits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "description",
                schema: "games",
                table: "games",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "rate_limit_per_minute",
                schema: "games",
                table: "games",
                type: "integer",
                nullable: false,
                defaultValue: 600);

            migrationBuilder.AddColumn<string>(
                name: "webhook_secret",
                schema: "games",
                table: "games",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "webhook_url",
                schema: "games",
                table: "games",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "game_files",
                schema: "games",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    game_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    content_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    size_bytes = table.Column<int>(type: "integer", nullable: false),
                    sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    content = table.Column<byte[]>(type: "bytea", nullable: false),
                    uploaded_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_game_files", x => x.id);
                    table.ForeignKey(
                        name: "fk_game_files_games_game_id",
                        column: x => x.game_id,
                        principalSchema: "games",
                        principalTable: "games",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "webhook_deliveries",
                schema: "games",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    game_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    created_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    attempts = table.Column<int>(type: "integer", nullable: false),
                    next_attempt_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    delivered_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_status_code = table.Column<int>(type: "integer", nullable: true),
                    last_error = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_webhook_deliveries", x => x.id);
                    table.ForeignKey(
                        name: "fk_webhook_deliveries_games_game_id",
                        column: x => x.game_id,
                        principalSchema: "games",
                        principalTable: "games",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_game_files_game_id_kind",
                schema: "games",
                table: "game_files",
                columns: new[] { "game_id", "kind" });

            migrationBuilder.CreateIndex(
                name: "ix_webhook_deliveries_game_id_created_on_utc",
                schema: "games",
                table: "webhook_deliveries",
                columns: new[] { "game_id", "created_on_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_webhook_deliveries_next_attempt_on_utc",
                schema: "games",
                table: "webhook_deliveries",
                column: "next_attempt_on_utc",
                filter: "next_attempt_on_utc IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "game_files",
                schema: "games");

            migrationBuilder.DropTable(
                name: "webhook_deliveries",
                schema: "games");

            migrationBuilder.DropColumn(
                name: "description",
                schema: "games",
                table: "games");

            migrationBuilder.DropColumn(
                name: "rate_limit_per_minute",
                schema: "games",
                table: "games");

            migrationBuilder.DropColumn(
                name: "webhook_secret",
                schema: "games",
                table: "games");

            migrationBuilder.DropColumn(
                name: "webhook_url",
                schema: "games",
                table: "games");
        }
    }
}
