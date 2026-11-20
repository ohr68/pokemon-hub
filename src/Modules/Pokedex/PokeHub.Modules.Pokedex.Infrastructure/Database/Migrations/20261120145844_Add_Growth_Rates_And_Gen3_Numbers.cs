using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokeHub.Modules.Pokedex.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Add_Growth_Rates_And_Gen3_Numbers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "gen3index",
                schema: "pokedex",
                table: "species",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "growth_rate",
                schema: "pokedex",
                table: "species",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "growth_rates",
                schema: "pokedex",
                columns: table => new
                {
                    name = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    experience_by_level = table.Column<int[]>(type: "integer[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_growth_rates", x => x.name);
                });

            migrationBuilder.CreateIndex(
                name: "ix_species_gen3index",
                schema: "pokedex",
                table: "species",
                column: "gen3index");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "growth_rates",
                schema: "pokedex");

            migrationBuilder.DropIndex(
                name: "ix_species_gen3index",
                schema: "pokedex",
                table: "species");

            migrationBuilder.DropColumn(
                name: "gen3index",
                schema: "pokedex",
                table: "species");

            migrationBuilder.DropColumn(
                name: "growth_rate",
                schema: "pokedex",
                table: "species");
        }
    }
}
