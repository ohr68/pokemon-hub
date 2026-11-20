using PokeHub.Modules.Games.Domain.Webhooks;
using PokeHub.Modules.Games.Infrastructure.Database;

namespace PokeHub.Modules.Games.Infrastructure.Webhooks;

internal sealed class WebhookDeliveryRepository(GamesDbContext context) : IWebhookDeliveryRepository
{
    public void Insert(WebhookDelivery delivery)
    {
        context.WebhookDeliveries.Add(delivery);
    }
}
