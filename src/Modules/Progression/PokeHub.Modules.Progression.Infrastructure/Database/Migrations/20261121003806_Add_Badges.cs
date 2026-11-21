using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokeHub.Modules.Progression.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Add_Badges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "badges",
                schema: "progression",
                table: "trainer_progress",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "trainer_badges",
                schema: "progression",
                columns: table => new
                {
                    trainer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    game_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    earned_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trainer_badges", x => new { x.trainer_id, x.game_id, x.code });
                    table.ForeignKey(
                        name: "fk_trainer_badges_trainer_progress_trainer_id",
                        column: x => x.trainer_id,
                        principalSchema: "progression",
                        principalTable: "trainer_progress",
                        principalColumn: "trainer_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trainer_games",
                schema: "progression",
                columns: table => new
                {
                    trainer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    game_id = table.Column<Guid>(type: "uuid", nullable: false),
                    game_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    total_badges = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trainer_games", x => new { x.trainer_id, x.game_id });
                    table.ForeignKey(
                        name: "fk_trainer_games_trainer_progress_trainer_id",
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
                name: "trainer_badges",
                schema: "progression");

            migrationBuilder.DropTable(
                name: "trainer_games",
                schema: "progression");

            migrationBuilder.DropColumn(
                name: "badges",
                schema: "progression",
                table: "trainer_progress");
        }
    }
}
