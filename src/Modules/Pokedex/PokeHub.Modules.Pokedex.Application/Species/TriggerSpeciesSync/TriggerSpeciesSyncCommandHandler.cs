using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Pokedex.Application.Abstractions.Sync;

namespace PokeHub.Modules.Pokedex.Application.Species.TriggerSpeciesSync;

// The sync takes a few seconds and calls PokéAPI around 30 times, so the request only starts
// the job; the job itself never runs twice at the same time.
internal sealed class TriggerSpeciesSyncCommandHandler(ISpeciesSyncScheduler speciesSyncScheduler)
    : ICommandHandler<TriggerSpeciesSyncCommand>
{
    public async Task<Result> Handle(TriggerSpeciesSyncCommand request, CancellationToken cancellationToken)
    {
        await speciesSyncScheduler.TriggerNowAsync(cancellationToken);

        return Result.Success();
    }
}
