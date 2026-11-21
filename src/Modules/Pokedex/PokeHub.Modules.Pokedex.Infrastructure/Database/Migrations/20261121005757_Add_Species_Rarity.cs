using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokeHub.Modules.Pokedex.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Add_Species_Rarity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_legendary",
                schema: "pokedex",
                table: "species",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_mythical",
                schema: "pokedex",
                table: "species",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_legendary",
                schema: "pokedex",
                table: "species");

            migrationBuilder.DropColumn(
                name: "is_mythical",
                schema: "pokedex",
                table: "species");
        }
    }
}
