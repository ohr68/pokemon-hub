using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Games.Domain.SaveEdits;

/// <summary>
/// A save PokeHub edited and handed back to a device, such as the 3DS app, to write into the
/// game. The Pokémon it put in are recorded as delivered, and the ones it took out as in the
/// hub, when it's handed out. If the device never wrote the save, the next one it sends shows
/// it: delivered ones missing go back to waiting, withdrawn ones still there go back inside the
/// game. Settled against the next save from that game.
/// </summary>
public sealed class SaveEdit : Entity
{
    private SaveEdit()
    {
    }

    public Guid Id { get; private set; }

    public Guid TrainerId { get; private set; }

    public Guid GameId { get; private set; }

    public DateTime CreatedOnUtc { get; private set; }

    // The Pokémon put into the save, and how the game knows each one there (same order).
    public List<Guid> DeliveredPokemonIds { get; private set; } = [];

    public List<string> DeliveredCatchKeys { get; private set; } = [];

    // Pokémon taken out of the save for the hub.
    public List<string> WithdrawnCatchKeys { get; private set; } = [];

    public DateTime? SettledOnUtc { get; private set; }

    public static SaveEdit Create(
        Guid trainerId,
        Guid gameId,
        IReadOnlyList<(Guid PokemonId, string CatchKey)> deliveries,
        IReadOnlyList<string> withdrawnCatchKeys,
        DateTime createdOnUtc)
    {
        ArgumentNullException.ThrowIfNull(deliveries);
        ArgumentNullException.ThrowIfNull(withdrawnCatchKeys);

        return new SaveEdit
        {
            Id = Guid.CreateVersion7(),
            TrainerId = trainerId,
            GameId = gameId,
            CreatedOnUtc = createdOnUtc,
            DeliveredPokemonIds = [.. deliveries.Select(d => d.PokemonId)],
            DeliveredCatchKeys = [.. deliveries.Select(d => d.CatchKey)],
            WithdrawnCatchKeys = [.. withdrawnCatchKeys],
        };
    }

    /// <summary>The delivered Pokémon that aren't in this save: the device didn't write it.</summary>
    public IReadOnlyList<Guid> MissingFrom(IReadOnlySet<string> presentCatchKeys)
    {
        ArgumentNullException.ThrowIfNull(presentCatchKeys);

        return [.. DeliveredPokemonIds.Where((_, i) => !presentCatchKeys.Contains(DeliveredCatchKeys[i]))];
    }

    /// <summary>The withdrawn Pokémon still in this save: the device didn't write it.</summary>
    public IReadOnlyList<string> StillIn(IReadOnlySet<string> presentCatchKeys)
    {
        ArgumentNullException.ThrowIfNull(presentCatchKeys);

        return [.. WithdrawnCatchKeys.Where(presentCatchKeys.Contains)];
    }

    public void Settle(DateTime settledOnUtc) => SettledOnUtc = settledOnUtc;
}
