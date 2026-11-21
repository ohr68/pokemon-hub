using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokeHub.Modules.Pokedex.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Add_Battle_Data : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "battle_data",
                schema: "pokedex",
                table: "pokemon",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "battle_data_updated_on_utc",
                schema: "pokedex",
                table: "pokemon",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "battle_data",
                schema: "pokedex",
                table: "pokemon");

            migrationBuilder.DropColumn(
                name: "battle_data_updated_on_utc",
                schema: "pokedex",
                table: "pokemon");
        }
    }
}
