using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokeHub.Modules.Games.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Add_Save_Files : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "save_files",
                schema: "games",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    trainer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    game_id = table.Column<Guid>(type: "uuid", nullable: false),
                    size_bytes = table.Column<int>(type: "integer", nullable: false),
                    sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    content = table.Column<byte[]>(type: "bytea", nullable: false),
                    uploaded_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_save_files", x => x.id);
                    table.ForeignKey(
                        name: "fk_save_files_games_game_id",
                        column: x => x.game_id,
                        principalSchema: "games",
                        principalTable: "games",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_save_files_game_id",
                schema: "games",
                table: "save_files",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "ix_save_files_trainer_id_game_id_uploaded_on_utc",
                schema: "games",
                table: "save_files",
                columns: new[] { "trainer_id", "game_id", "uploaded_on_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "save_files",
                schema: "games");
        }
    }
}
