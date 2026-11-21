using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PokeHub.Modules.Progression.Domain.Trainers;

namespace PokeHub.Modules.Progression.Infrastructure.Trainers;

internal sealed class TrainerProgressConfiguration : IEntityTypeConfiguration<TrainerProgress>
{
    public void Configure(EntityTypeBuilder<TrainerProgress> builder)
    {
        builder.ToTable("trainer_progress");

        // The Pokédex's trainer id; the trainer lives in that module, so no foreign key.
        builder.HasKey(p => p.TrainerId);

        builder.Property(p => p.TrainerId).ValueGeneratedNever();

        builder.Ignore(p => p.NewEntries);

        // Which trophies the trainer earned; what each one is lives in code (Trophies).
        builder.OwnsMany(p => p.Trophies, trophies =>
        {
            trophies.ToTable("trainer_trophies");
            trophies.WithOwner().HasForeignKey(t => t.TrainerId);
            trophies.HasKey(t => new { t.TrainerId, t.TrophyCode });
            trophies.Property(t => t.TrophyCode).HasMaxLength(50);
        });

        builder.Navigation(p => p.Trophies).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(p => p.Games, games =>
        {
            games.ToTable("trainer_games");
            games.WithOwner().HasForeignKey(g => g.TrainerId);
            // The game lives in the Games module, so no foreign key to it.
            games.HasKey(g => new { g.TrainerId, g.GameId });
            // Set by us: without this EF treats a Guid in an owned key as generated, takes a
            // new game for an existing one and updates instead of inserting.
            games.Property(g => g.GameId).ValueGeneratedNever();
            games.Property(g => g.GameName).HasMaxLength(100);
        });

        builder.Navigation(p => p.Games).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(p => p.EarnedBadges, badges =>
        {
            badges.ToTable("trainer_badges");
            badges.WithOwner().HasForeignKey(b => b.TrainerId);
            badges.HasKey(b => new { b.TrainerId, b.GameId, b.Code });
            badges.Property(b => b.GameId).ValueGeneratedNever();
            badges.Property(b => b.Code).HasMaxLength(50);
            badges.Property(b => b.Name).HasMaxLength(100);
        });

        builder.Navigation(p => p.EarnedBadges).UsePropertyAccessMode(PropertyAccessMode.Field);

        // Two awards for the same trainer at the same moment can't both win.
        builder.Property<uint>("Version").IsRowVersion();
    }
}

internal sealed class XpEntryConfiguration : IEntityTypeConfiguration<XpEntry>
{
    public void Configure(EntityTypeBuilder<XpEntry> builder)
    {
        builder.ToTable("xp_entries");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.Reason).HasConversion<string>().HasMaxLength(30);

        builder.Property(e => e.Source).HasMaxLength(150);

        builder.HasOne<TrainerProgress>().WithMany().HasForeignKey(e => e.TrainerId);

        // Each thing pays XP once.
        builder.HasIndex(e => new { e.TrainerId, e.Source }).IsUnique();

        builder.HasIndex(e => new { e.TrainerId, e.EarnedOnUtc });
    }
}
