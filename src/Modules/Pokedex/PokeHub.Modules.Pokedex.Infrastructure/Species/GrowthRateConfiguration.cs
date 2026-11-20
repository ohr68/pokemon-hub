using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PokeHub.Modules.Pokedex.Domain.Species;

namespace PokeHub.Modules.Pokedex.Infrastructure.Species;

internal sealed class GrowthRateConfiguration : IEntityTypeConfiguration<GrowthRate>
{
    public void Configure(EntityTypeBuilder<GrowthRate> builder)
    {
        builder.ToTable("growth_rates");

        builder.HasKey(r => r.Name);

        builder.Property(r => r.Name).HasMaxLength(30);

        // A PostgreSQL integer[] of 100 values.
        builder.Property(r => r.ExperienceByLevel);
    }
}
