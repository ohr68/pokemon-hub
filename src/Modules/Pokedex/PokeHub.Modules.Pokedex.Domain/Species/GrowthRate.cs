namespace PokeHub.Modules.Pokedex.Domain.Species;

/// <summary>
/// A growth rate from PokéAPI: the total experience needed to reach each level. Games that
/// store experience but not level (Gen 3 PC boxes, for example) use it to work the level out.
/// </summary>
public sealed class GrowthRate
{
    public const int MaxLevel = 100;

    private GrowthRate()
    {
    }

    public string Name { get; private set; }

    /// <summary>
    /// Index 0 is level 1; each value is the total experience needed for that level.
    /// </summary>
    public int[] ExperienceByLevel { get; private set; }

    public static GrowthRate Create(string name, int[] experienceByLevel)
    {
        return new GrowthRate { Name = name, ExperienceByLevel = experienceByLevel };
    }

    public bool Update(int[] experienceByLevel)
    {
        if (ExperienceByLevel.SequenceEqual(experienceByLevel))
        {
            return false;
        }

        ExperienceByLevel = experienceByLevel;

        return true;
    }
}
