namespace PokeHub.Modules.Pokedex.Application.Trainers.GetTrainer;

/// <param name="Code">The public code other trainers use to find this one.</param>
/// <param name="StarterSpeciesId">Empty until the trainer chooses a starter.</param>
/// <param name="AvatarPokemonId">The Pokémon shown as the trainer's picture, while they own it.</param>
/// <param name="AvatarSpeciesId">Its species now; it may have evolved since it was chosen.</param>
/// <param name="AvatarIsShiny">Whether it is shiny.</param>
public sealed record TrainerResponse(
    Guid Id,
    string Name,
    string Code,
    int? StarterSpeciesId,
    Guid? AvatarPokemonId,
    int? AvatarSpeciesId,
    bool? AvatarIsShiny);
