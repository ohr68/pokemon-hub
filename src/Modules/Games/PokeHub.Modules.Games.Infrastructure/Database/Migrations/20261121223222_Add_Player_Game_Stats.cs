using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokeHub.Modules.Games.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Add_Player_Game_Stats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "player_game_stats",
                schema: "games",
                columns: table => new
                {
                    trainer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    game_id = table.Column<Guid>(type: "uuid", nullable: false),
                    play_time_seconds = table.Column<long>(type: "bigint", nullable: false),
                    money = table.Column<long>(type: "bigint", nullable: false),
                    pokedex_seen = table.Column<int>(type: "integer", nullable: false),
                    pokedex_owned = table.Column<int>(type: "integer", nullable: false),
                    counters = table.Column<string>(type: "jsonb", nullable: false),
                    updated_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_player_game_stats", x => new { x.trainer_id, x.game_id });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "player_game_stats",
                schema: "games");
        }
    }
}
