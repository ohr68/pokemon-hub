using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Pokedex.Domain.Pokemon;

public static class PokemonErrors
{
    public static Error NotFound(Guid pokemonId)
    {
        return Error.NotFound("Pokemon.NotFound", $"The Pokémon with the identifier {pokemonId} was not found.");
    }

    public static Error SpeciesNotInGame(int speciesId, string gameName)
    {
        return Error.Problem("Pokemon.SpeciesNotInGame", $"Pokémon #{speciesId} doesn't exist in {gameName}.");
    }

    public static Error NotOwnedByTrainer(Guid pokemonId)
    {
        return Error.Problem("Pokemon.NotOwnedByTrainer", $"The Pokémon {pokemonId} belongs to another trainer.");
    }

    public static Error NotActive(Guid pokemonId)
    {
        return Error.Conflict("Pokemon.NotActive", $"The Pokémon {pokemonId} has already left its trainer.");
    }

    public static Error InvalidLevel(int level)
    {
        return Error.Problem(
            "Pokemon.InvalidLevel",
            $"The level {level} must be between {OwnedPokemon.MinLevel} and {OwnedPokemon.MaxLevel}.");
    }

    public static Error CatchKeyInUse(string catchKey)
    {
        return Error.Conflict("Pokemon.CatchKeyInUse", $"The catch key {catchKey} was already used for another trainer.");
    }

    public static readonly Error NotInHub = Error.Conflict(
        "Pokemon.NotInHub",
        "Only Pokémon in the PokeHub hub can be traded or released there. Move it out of the game first.");

    public static readonly Error NotInThisSave = Error.Conflict(
        "Pokemon.NotInThisSave",
        "The Pokémon is not in this game's save.");

    public static readonly Error InsideGame = Error.Conflict(
        "Pokemon.InsideGame",
        "The Pokémon is inside the game's own save. Send it to PokeHub from inside the game.");

    public static readonly Error NotAwaitingDelivery = Error.Conflict(
        "Pokemon.NotAwaitingDelivery",
        "The Pokémon isn't waiting to be delivered into this game.");

    public static readonly Error AlreadyThere = Error.Conflict(
        "Pokemon.AlreadyThere",
        "The Pokémon is already there.");

    public static readonly Error TradeConflict = Error.Conflict(
        "Pokemon.TradeConflict",
        "One of the Pokémon changed while the trade was being made. Try again.");

    public static readonly Error ChangedMeanwhile = Error.Conflict(
        "Pokemon.ChangedMeanwhile",
        "One of the Pokémon changed on PokeHub at the same time. Try again.");
}
