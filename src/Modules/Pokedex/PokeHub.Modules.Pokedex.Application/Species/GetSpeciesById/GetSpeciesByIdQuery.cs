using PokeHub.Common.Application.Messaging;
using PokeHub.Modules.Pokedex.Application.Species.GetSpecies;

namespace PokeHub.Modules.Pokedex.Application.Species.GetSpeciesById;

public sealed record GetSpeciesByIdQuery(int SpeciesId) : IQuery<SpeciesResponse>;
