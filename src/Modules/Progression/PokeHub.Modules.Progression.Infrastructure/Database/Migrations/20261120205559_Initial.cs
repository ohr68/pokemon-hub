using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokeHub.Modules.Progression.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "progression");

            migrationBuilder.CreateTable(
                name: "inbox_message_consumers",
                schema: "progression",
                columns: table => new
                {
                    inbox_message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inbox_message_consumers", x => new { x.inbox_message_id, x.name });
                });

            migrationBuilder.CreateTable(
                name: "inbox_messages",
                schema: "progression",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<string>(type: "jsonb", maxLength: 2000, nullable: false),
                    occurred_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_message_consumers",
                schema: "progression",
                columns: table => new
                {
                    outbox_message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_message_consumers", x => new { x.outbox_message_id, x.name });
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "progression",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<string>(type: "jsonb", maxLength: 2000, nullable: false),
                    occurred_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "trainer_progress",
                schema: "progression",
                columns: table => new
                {
                    trainer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    total_xp = table.Column<long>(type: "bigint", nullable: false),
                    level = table.Column<int>(type: "integer", nullable: false),
                    updated_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                    // No xmin column: it's PostgreSQL's own system column, mapped as the row version.
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trainer_progress", x => x.trainer_id);
                });

            migrationBuilder.CreateTable(
                name: "xp_entries",
                schema: "progression",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    trainer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reason = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    amount = table.Column<int>(type: "integer", nullable: false),
                    source = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    earned_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_xp_entries", x => x.id);
                    table.ForeignKey(
                        name: "fk_xp_entries_trainer_progress_trainer_id",
                        column: x => x.trainer_id,
                        principalSchema: "progression",
                        principalTable: "trainer_progress",
                        principalColumn: "trainer_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_xp_entries_trainer_id_earned_on_utc",
                schema: "progression",
                table: "xp_entries",
                columns: new[] { "trainer_id", "earned_on_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_xp_entries_trainer_id_source",
                schema: "progression",
                table: "xp_entries",
                columns: new[] { "trainer_id", "source" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inbox_message_consumers",
                schema: "progression");

            migrationBuilder.DropTable(
                name: "inbox_messages",
                schema: "progression");

            migrationBuilder.DropTable(
                name: "outbox_message_consumers",
                schema: "progression");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "progression");

            migrationBuilder.DropTable(
                name: "xp_entries",
                schema: "progression");

            migrationBuilder.DropTable(
                name: "trainer_progress",
                schema: "progression");
        }
    }
}
