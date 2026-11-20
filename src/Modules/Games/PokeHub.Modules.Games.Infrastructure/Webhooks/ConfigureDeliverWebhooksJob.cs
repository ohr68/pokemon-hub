using Microsoft.Extensions.Options;
using Quartz;

namespace PokeHub.Modules.Games.Infrastructure.Webhooks;

internal sealed class ConfigureDeliverWebhooksJob : IConfigureOptions<QuartzOptions>
{
    // Events reach games within seconds; retries wait longer (see WebhookDelivery.Failed).
    private const int IntervalInSeconds = 5;

    public void Configure(QuartzOptions options)
    {
        string jobName = typeof(DeliverWebhooksJob).FullName!;

        options
            .AddJob<DeliverWebhooksJob>(configure => configure.WithIdentity(jobName))
            .AddTrigger(configure =>
                configure
                    .ForJob(jobName)
                    .WithSimpleSchedule(schedule => schedule.WithIntervalInSeconds(IntervalInSeconds).RepeatForever()));
    }
}
