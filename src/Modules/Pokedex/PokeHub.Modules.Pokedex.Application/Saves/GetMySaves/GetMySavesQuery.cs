using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Pokedex.Application.Saves.GetMySaves;

public sealed record GetMySavesQuery(Guid TrainerId) : IQuery<IReadOnlyCollection<SaveResponse>>;
