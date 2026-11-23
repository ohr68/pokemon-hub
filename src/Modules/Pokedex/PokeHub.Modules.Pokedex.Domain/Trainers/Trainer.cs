using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Pokedex.Domain.Trainers;

/// <summary>
/// A PokeHub user as seen by the Pokédex. Shares its identifier with the user.
/// </summary>
public sealed class Trainer : Entity
{
    /// <summary>
    /// The classic starters of every main-series generation (grass, fire, water).
    /// </summary>
    public static readonly IReadOnlySet<int> Starters = new HashSet<int>
    {
        1, 4, 7,
        152, 155, 158,
        252, 255, 258,
        387, 390, 393,
        495, 498, 501,
        650, 653, 656,
        722, 725, 728,
        810, 813, 816,
        906, 909, 912,
    };

    private Trainer()
    {
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public string Code { get; private set; }

    public int? StarterSpeciesId { get; private set; }

    /// <summary>
    /// The Pokémon shown as the trainer's picture. A reference, so the picture follows it when it
    /// evolves; it stops showing once the trainer no longer owns it.
    /// </summary>
    public Guid? AvatarPokemonId { get; private set; }

    public static Trainer Create(Guid id, string name)
    {
        return new Trainer
        {
            Id = id,
            Name = name,
            Code = TrainerCode.New(),
        };
    }

    public Result ChooseStarter(int speciesId)
    {
        if (StarterSpeciesId is not null)
        {
            return Result.Failure(TrainerErrors.StarterAlreadyChosen);
        }

        if (!Starters.Contains(speciesId))
        {
            return Result.Failure(TrainerErrors.InvalidStarter(speciesId));
        }

        StarterSpeciesId = speciesId;

        return Result.Success();
    }

    public void SetAvatar(Guid? pokemonId)
    {
        AvatarPokemonId = pokemonId;
    }

    public void Rename(string name)
    {
        Name = name;
    }
}
