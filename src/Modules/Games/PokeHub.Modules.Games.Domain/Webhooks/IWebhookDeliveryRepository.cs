namespace PokeHub.Modules.Games.Domain.Webhooks;

public interface IWebhookDeliveryRepository
{
    void Insert(WebhookDelivery delivery);
}
