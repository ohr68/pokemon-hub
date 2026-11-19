using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Pokedex.Application.Progress.GetPokedexProgress;

public sealed record GetPokedexProgressQuery(Guid TrainerId) : IQuery<PokedexProgressResponse>;
