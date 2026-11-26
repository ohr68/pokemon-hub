using System.Text.RegularExpressions;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Pokedex.Application.Sprites;

/// <summary>
/// Where PokeHub fetches the sprites PokéAPI publishes, for devices that show them without
/// reaching the internet themselves (the 3DS app). Small ones: 96×96 for Pokémon, 30×30 for items.
/// </summary>
public interface ISpriteSource
{
    /// <summary>The PNG at this path under PokéAPI's sprites, or null when there's none.</summary>
    Task<byte[]?> GetAsync(string path, CancellationToken cancellationToken = default);
}

/// <summary>A Pokémon's small front sprite, shiny or not.</summary>
public sealed record GetPokemonSpriteQuery(int SpeciesId, bool IsShiny) : IQuery<byte[]>;

/// <summary>An item's sprite, by PokéAPI's name for it (poke-ball, rare-candy).</summary>
public sealed record GetItemSpriteQuery(string Name) : IQuery<byte[]>;

public static partial class SpriteErrors
{
    public static readonly Error NotFound = Error.NotFound("Sprites.NotFound", "There's no sprite for that.");
}

internal sealed partial class GetSpriteQueryHandlers(ISpriteSource spriteSource)
    : IQueryHandler<GetPokemonSpriteQuery, byte[]>, IQueryHandler<GetItemSpriteQuery, byte[]>
{
    private const int LastSpecies = 1025;

    public async Task<Result<byte[]>> Handle(GetPokemonSpriteQuery request, CancellationToken cancellationToken)
    {
        if (request.SpeciesId is < 1 or > LastSpecies)
        {
            return Result.Failure<byte[]>(SpriteErrors.NotFound);
        }

        string path = request.IsShiny ? $"pokemon/shiny/{request.SpeciesId}.png" : $"pokemon/{request.SpeciesId}.png";
        return await FetchAsync(path, cancellationToken);
    }

    public async Task<Result<byte[]>> Handle(GetItemSpriteQuery request, CancellationToken cancellationToken)
    {
        // Only PokéAPI's own names, so nothing else can be asked for.
        return ItemName().IsMatch(request.Name)
            ? await FetchAsync($"items/{request.Name}.png", cancellationToken)
            : Result.Failure<byte[]>(SpriteErrors.NotFound);
    }

    private async Task<Result<byte[]>> FetchAsync(string path, CancellationToken cancellationToken)
    {
        byte[]? png = await spriteSource.GetAsync(path, cancellationToken);
        return png is null ? Result.Failure<byte[]>(SpriteErrors.NotFound) : png;
    }

    [GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$")]
    private static partial Regex ItemName();
}
