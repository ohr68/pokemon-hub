using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokeHub.Modules.Economy.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Add_Item_Shop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "item_offers",
                schema: "economy",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_id = table.Column<int>(type: "integer", nullable: false),
                    item_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    pocket = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<int>(type: "integer", nullable: false),
                    stock = table.Column<int>(type: "integer", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_item_offers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "item_purchases",
                schema: "economy",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    trainer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    offer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_id = table.Column<int>(type: "integer", nullable: false),
                    item_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    pocket = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<int>(type: "integer", nullable: false),
                    purchased_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    delivered_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_item_purchases", x => x.id);
                    table.ForeignKey(
                        name: "fk_item_purchases_item_offers_offer_id",
                        column: x => x.offer_id,
                        principalSchema: "economy",
                        principalTable: "item_offers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_item_purchases_offer_id",
                schema: "economy",
                table: "item_purchases",
                column: "offer_id");

            migrationBuilder.CreateIndex(
                name: "ix_item_purchases_trainer_id_delivered_on_utc",
                schema: "economy",
                table: "item_purchases",
                columns: new[] { "trainer_id", "delivered_on_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "item_purchases",
                schema: "economy");

            migrationBuilder.DropTable(
                name: "item_offers",
                schema: "economy");
        }
    }
}
