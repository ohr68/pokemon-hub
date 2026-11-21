using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokeHub.Modules.Progression.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Fix_Badge_Keys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Nothing changes in the database: the game ids in trainer_games and trainer_badges
            // are now marked as set by the app (not generated), which only EF's model records.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Nothing to undo in the database (see Up).
        }
    }
}
