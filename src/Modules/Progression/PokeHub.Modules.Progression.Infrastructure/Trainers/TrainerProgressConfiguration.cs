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
