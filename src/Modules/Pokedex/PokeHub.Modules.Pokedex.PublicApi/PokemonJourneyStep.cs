namespace PokeHub.Modules.Pokedex.PublicApi;

/// <summary>Something in a Pokémon's history with a trainer.</summary>
/// <param name="Change">What happened, such as MovedToHub, MovedToSave or EvolvedByTrade.</param>
/// <param name="GameId">The game it happened in; empty when it happened in the hub.</param>
public sealed record PokemonJourneyStep(Guid TrainerId, Guid PokemonId, string Change, Guid? GameId, DateTime OccurredOnUtc);
