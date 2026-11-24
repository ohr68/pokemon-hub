using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokeHub.Modules.Economy.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Add_Item_Purchase_Game : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "game_id",
                schema: "economy",
                table: "item_purchases",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "game_name",
                schema: "economy",
                table: "item_purchases",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "game_id",
                schema: "economy",
                table: "item_purchases");

            migrationBuilder.DropColumn(
                name: "game_name",
                schema: "economy",
                table: "item_purchases");
        }
    }
}
