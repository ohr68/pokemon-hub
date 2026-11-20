using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PokeHub.Modules.Games.Domain.Games;
using PokeHub.Modules.Games.Domain.Webhooks;

namespace PokeHub.Modules.Games.Infrastructure.Webhooks;

internal sealed class WebhookDeliveryConfiguration : IEntityTypeConfiguration<WebhookDelivery>
{
    public void Configure(EntityTypeBuilder<WebhookDelivery> builder)
    {
        builder.ToTable("webhook_deliveries");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id).ValueGeneratedNever();

        builder.Property(d => d.EventType).HasMaxLength(50);

        builder.Property(d => d.Payload).HasColumnType("jsonb");

        builder.Property(d => d.LastError).HasMaxLength(500);

        builder.HasOne<Game>().WithMany().HasForeignKey(d => d.GameId);

        // The sender only looks at deliveries still waiting.
        builder.HasIndex(d => d.NextAttemptOnUtc).HasFilter("next_attempt_on_utc IS NOT NULL");

        builder.HasIndex(d => new { d.GameId, d.CreatedOnUtc });
    }
}
