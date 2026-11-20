using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Pokedex.Domain.Saves;

/// <summary>
/// A trainer's progress in one game: the box where that game keeps their Pokémon.
/// Every Pokémon is either in the PokeHub hub or in exactly one save; moving between
/// games goes through the hub, like Pokémon HOME.
/// </summary>
public sealed class Save : Entity
{
    private Save()
    {
    }

    public Guid Id { get; private set; }

    public Guid TrainerId { get; private set; }

    public Guid GameId { get; private set; }

    // Copied from the Games module so the Pokédex can show it without asking.
    public string GameName { get; private set; }

    public DateTime CreatedOnUtc { get; private set; }

    public static Save Create(Guid trainerId, Guid gameId, string gameName, DateTime utcNow)
    {
        return new Save
        {
            Id = Guid.CreateVersion7(),
            TrainerId = trainerId,
            GameId = gameId,
            GameName = gameName,
            CreatedOnUtc = utcNow,
        };
    }
}
