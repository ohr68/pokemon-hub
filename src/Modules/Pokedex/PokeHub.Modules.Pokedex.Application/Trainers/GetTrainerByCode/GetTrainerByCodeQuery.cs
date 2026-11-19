using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Pokedex.Application.Trainers.GetTrainerByCode;

/// <summary>
/// Looks a trainer up by their public code, with the Pokémon they could trade.
/// </summary>
public sealed record GetTrainerByCodeQuery(string Code) : IQuery<TrainerShowcaseResponse>;
