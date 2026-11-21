using Microsoft.EntityFrameworkCore;
using PokeHub.Modules.Games.Domain.Stats;
using PokeHub.Modules.Games.Infrastructure.Database;

namespace PokeHub.Modules.Games.Infrastructure.Stats;

internal sealed class PlayerGameStatsRepository(GamesDbContext context) : IPlayerGameStatsRepository
{
    public Task<PlayerGameStats?> GetAsync(Guid trainerId, Guid gameId, CancellationToken cancellationToken = default)
    {
        return context.PlayerGameStats.SingleOrDefaultAsync(s => s.TrainerId == trainerId && s.GameId == gameId, cancellationToken);
    }

    public void Insert(PlayerGameStats stats)
    {
        context.PlayerGameStats.Add(stats);
    }
}

internal sealed class PlayerGameStatsConfiguration : IEntityTypeConfiguration<PlayerGameStats>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<PlayerGameStats> builder)
    {
        builder.ToTable("player_game_stats");

        builder.HasKey(s => new { s.TrainerId, s.GameId });

        builder.Property(s => s.TrainerId).ValueGeneratedNever();

        builder.Property(s => s.GameId).ValueGeneratedNever();

        builder.Property(s => s.Counters).HasColumnType("jsonb");
    }
}
