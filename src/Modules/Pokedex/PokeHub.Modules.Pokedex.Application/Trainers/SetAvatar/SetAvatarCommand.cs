using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Pokedex.Application.Trainers.SetAvatar;

/// <param name="PokemonId">One of the trainer's Pokémon, or empty to remove the picture.</param>
public sealed record SetAvatarCommand(Guid TrainerId, Guid? PokemonId) : ICommand;
