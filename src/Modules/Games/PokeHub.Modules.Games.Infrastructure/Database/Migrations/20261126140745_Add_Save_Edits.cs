using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokeHub.Modules.Games.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Add_Save_Edits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "save_edits",
                schema: "games",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    trainer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    game_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    delivered_pokemon_ids = table.Column<List<Guid>>(type: "uuid[]", nullable: false),
                    delivered_catch_keys = table.Column<List<string>>(type: "text[]", nullable: false),
                    settled_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_save_edits", x => x.id);
                    table.ForeignKey(
                        name: "fk_save_edits_games_game_id",
                        column: x => x.game_id,
                        principalSchema: "games",
                        principalTable: "games",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_save_edits_game_id",
                schema: "games",
                table: "save_edits",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "ix_save_edits_trainer_id_game_id_settled_on_utc",
                schema: "games",
                table: "save_edits",
                columns: new[] { "trainer_id", "game_id", "settled_on_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "save_edits",
                schema: "games");
        }
    }
}
