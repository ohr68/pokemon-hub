using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokeHub.Modules.Economy.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "economy");

            migrationBuilder.CreateTable(
                name: "inbox_message_consumers",
                schema: "economy",
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
                schema: "economy",
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
                schema: "economy",
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
                schema: "economy",
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
                name: "shop_offers",
                schema: "economy",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    species_id = table.Column<int>(type: "integer", nullable: false),
                    species_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_rare = table.Column<bool>(type: "boolean", nullable: false),
                    level = table.Column<int>(type: "integer", nullable: false),
                    is_shiny = table.Column<bool>(type: "boolean", nullable: false),
                    price = table.Column<int>(type: "integer", nullable: false),
                    stock = table.Column<int>(type: "integer", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                    // No xmin column: it's PostgreSQL's own system column, mapped as the row version.
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shop_offers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "wallets",
                schema: "economy",
                columns: table => new
                {
                    trainer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    balance = table.Column<long>(type: "bigint", nullable: false),
                    updated_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                    // No xmin column: it's PostgreSQL's own system column, mapped as the row version.
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_wallets", x => x.trainer_id);
                });

            migrationBuilder.CreateTable(
                name: "purchases",
                schema: "economy",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    trainer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    offer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    species_id = table.Column<int>(type: "integer", nullable: false),
                    species_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    level = table.Column<int>(type: "integer", nullable: false),
                    is_shiny = table.Column<bool>(type: "boolean", nullable: false),
                    price = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    pokemon_id = table.Column<Guid>(type: "uuid", nullable: true),
                    purchased_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_purchases", x => x.id);
                    table.ForeignKey(
                        name: "fk_purchases_shop_offers_offer_id",
                        column: x => x.offer_id,
                        principalSchema: "economy",
                        principalTable: "shop_offers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "coin_entries",
                schema: "economy",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    trainer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reason = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    amount = table.Column<int>(type: "integer", nullable: false),
                    source = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    occurred_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_coin_entries", x => x.id);
                    table.ForeignKey(
                        name: "fk_coin_entries_wallets_trainer_id",
                        column: x => x.trainer_id,
                        principalSchema: "economy",
                        principalTable: "wallets",
                        principalColumn: "trainer_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_coin_entries_trainer_id_occurred_on_utc",
                schema: "economy",
                table: "coin_entries",
                columns: new[] { "trainer_id", "occurred_on_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_coin_entries_trainer_id_source",
                schema: "economy",
                table: "coin_entries",
                columns: new[] { "trainer_id", "source" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_purchases_offer_id",
                schema: "economy",
                table: "purchases",
                column: "offer_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchases_trainer_id_purchased_on_utc",
                schema: "economy",
                table: "purchases",
                columns: new[] { "trainer_id", "purchased_on_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "coin_entries",
                schema: "economy");

            migrationBuilder.DropTable(
                name: "inbox_message_consumers",
                schema: "economy");

            migrationBuilder.DropTable(
                name: "inbox_messages",
                schema: "economy");

            migrationBuilder.DropTable(
                name: "outbox_message_consumers",
                schema: "economy");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "economy");

            migrationBuilder.DropTable(
                name: "purchases",
                schema: "economy");

            migrationBuilder.DropTable(
                name: "wallets",
                schema: "economy");

            migrationBuilder.DropTable(
                name: "shop_offers",
                schema: "economy");
        }
    }
}
