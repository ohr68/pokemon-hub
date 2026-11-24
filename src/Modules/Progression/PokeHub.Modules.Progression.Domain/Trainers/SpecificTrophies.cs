namespace PokeHub.Modules.Progression.Domain.Trainers;

/// <summary>What a hand-made trophy counts.</summary>
public enum SpecificStat
{
    HallOfFame = 1,
    SpeciesCaught = 2,
    SpeciesEvolved = 3,
    MostGamesVisitedByOnePokemon = 4,
    ReturnsFromHub = 5,
    TradeEvolutions = 6,
}

/// <summary>A hand-made goal, for one game or for the hub as a whole.</summary>
/// <param name="Code">Stable id: never rename one, and never use a colon in it.</param>
/// <param name="GameSlug">The game it belongs to; empty for the hub.</param>
/// <param name="SpeciesIds">The species a species goal is about.</param>
public sealed record SpecificTrophy(
    string Code,
    string Name,
    string Description,
    TrophyTier Tier,
    SpecificStat Stat,
    int Target,
    string? GameSlug = null,
    IReadOnlyList<int>? SpeciesIds = null);

public static class SpecificTrophies
{
    private const string FireRed = "firered";
    private const string LeafGreen = "leafgreen";
    private const string Ruby = "ruby";

    public static readonly IReadOnlyList<SpecificTrophy> All =
    [
        new("firered-champion", "Campeão de Kanto", "Derrote a Elite dos Quatro em FireRed.", TrophyTier.Gold, SpecificStat.HallOfFame, 1, FireRed),
        new("firered-birds", "Aves lendárias", "Capture Articuno, Zapdos e Moltres em FireRed.", TrophyTier.Gold, SpecificStat.SpeciesCaught, 3, FireRed, [144, 145, 146]),
        new("firered-mewtwo", "O Pokémon genético", "Capture Mewtwo em FireRed.", TrophyTier.Gold, SpecificStat.SpeciesCaught, 1, FireRed, [150]),
        new("firered-eevee", "Família Eevee", "Evolua Vaporeon, Jolteon e Flareon em FireRed.", TrophyTier.Silver, SpecificStat.SpeciesEvolved, 3, FireRed, [134, 135, 136]),

        new("leafgreen-champion", "Campeão de Kanto", "Derrote a Elite dos Quatro em LeafGreen.", TrophyTier.Gold, SpecificStat.HallOfFame, 1, LeafGreen),
        new("leafgreen-birds", "Aves lendárias", "Capture Articuno, Zapdos e Moltres em LeafGreen.", TrophyTier.Gold, SpecificStat.SpeciesCaught, 3, LeafGreen, [144, 145, 146]),
        new("leafgreen-mewtwo", "O Pokémon genético", "Capture Mewtwo em LeafGreen.", TrophyTier.Gold, SpecificStat.SpeciesCaught, 1, LeafGreen, [150]),

        // The Pokémon only LeafGreen has in the wild; FireRed players get them through the hub.
        new("leafgreen-exclusives", "Só no LeafGreen", "Capture Sandshrew, Vulpix, Bellsprout, Slowpoke, Staryu, Magmar e Pinsir em LeafGreen.", TrophyTier.Silver, SpecificStat.SpeciesCaught, 7, LeafGreen, [27, 37, 69, 79, 120, 126, 127]),

        new("ruby-champion", "Campeão de Hoenn", "Derrote a Elite dos Quatro em Ruby.", TrophyTier.Gold, SpecificStat.HallOfFame, 1, Ruby),
        new("ruby-groudon", "Senhor da terra", "Capture Groudon em Ruby.", TrophyTier.Gold, SpecificStat.SpeciesCaught, 1, Ruby, [383]),
        new("ruby-rayquaza", "Senhor dos céus", "Capture Rayquaza em Ruby.", TrophyTier.Gold, SpecificStat.SpeciesCaught, 1, Ruby, [384]),
        new("ruby-regis", "Os três Regis", "Capture Regirock, Regice e Registeel em Ruby.", TrophyTier.Gold, SpecificStat.SpeciesCaught, 3, Ruby, [377, 378, 379]),

        // The Pokémon only Ruby has in the wild; Sapphire players get them through the hub.
        new("ruby-exclusives", "Só no Ruby", "Capture Seedot, Mawile, Zangoose e Solrock em Ruby.", TrophyTier.Silver, SpecificStat.SpeciesCaught, 4, Ruby, [273, 303, 335, 338]),

        new("hub-round-trip", "Ida e volta", "Traga um Pokémon do hub de volta para um jogo.", TrophyTier.Bronze, SpecificStat.ReturnsFromHub, 1),
        new("hub-trade-evolve", "Evolução por troca", "Receba um Pokémon que evolui na troca.", TrophyTier.Silver, SpecificStat.TradeEvolutions, 1),
        new("hub-world-traveler", "Pokémon viajante", "Leve o mesmo Pokémon a 3 jogos.", TrophyTier.Gold, SpecificStat.MostGamesVisitedByOnePokemon, 3),
    ];
}

public enum JourneyKind
{
    Other = 0,
    MovedToHub = 1,
    MovedToSave = 2,
    EvolvedByTrade = 3,
}

/// <summary>Something in one Pokémon's history with the trainer.</summary>
/// <param name="GameId">The game it happened in; empty when it happened in the hub.</param>
public sealed record JourneyMove(Guid PokemonId, JourneyKind Kind, Guid? GameId, DateTime OccurredOnUtc);

/// <summary>What hand-made trophies measure for one trainer.</summary>
/// <param name="GameIds">Registered games by slug.</param>
/// <param name="HallOfFame">Hall of Fame entries each game reported.</param>
/// <param name="ConnectedGameIds">Games with a known Pokédex, the ones a journey counts in.</param>
public sealed record SpecificProgress(
    IReadOnlyCollection<AchievementFact> Facts,
    IReadOnlyCollection<JourneyMove> Journeys,
    IReadOnlyDictionary<string, Guid> GameIds,
    IReadOnlyDictionary<Guid, long> HallOfFame,
    IReadOnlySet<Guid> ConnectedGameIds)
{
    public int Value(SpecificTrophy trophy) => trophy.Stat switch
    {
        SpecificStat.HallOfFame => GameOf(trophy) is { } game
            ? (int)Math.Min(HallOfFame.GetValueOrDefault(game), int.MaxValue)
            : 0,
        SpecificStat.SpeciesCaught => SpeciesIn(trophy, isEvolution: false),
        SpecificStat.SpeciesEvolved => SpeciesIn(trophy, isEvolution: true),
        SpecificStat.MostGamesVisitedByOnePokemon => MostGamesVisited(),
        SpecificStat.ReturnsFromHub => ReturnsFromHub(),
        SpecificStat.TradeEvolutions => Journeys.Count(j => j.Kind == JourneyKind.EvolvedByTrade),
        _ => 0,
    };

    private Guid? GameOf(SpecificTrophy trophy) =>
        trophy.GameSlug is not null && GameIds.TryGetValue(trophy.GameSlug, out Guid game) ? game : null;

    private int SpeciesIn(SpecificTrophy trophy, bool isEvolution)
    {
        if (GameOf(trophy) is not { } game || trophy.SpeciesIds is null)
        {
            return 0;
        }

        return Facts
            .Where(f => f.GameId == game && f.IsEvolution == isEvolution && trophy.SpeciesIds.Contains(f.SpeciesId))
            .Select(f => f.SpeciesId)
            .Distinct()
            .Count();
    }

    private int MostGamesVisited() =>
        Journeys
            .Where(j => j.GameId is { } game && ConnectedGameIds.Contains(game))
            .GroupBy(j => j.PokemonId)
            .Select(pokemon => pokemon.Select(j => j.GameId).Distinct().Count())
            .DefaultIfEmpty()
            .Max();

    // A move into a game counts when the Pokémon had come out to the hub before it.
    private int ReturnsFromHub()
    {
        int returns = 0;

        foreach (IGrouping<Guid, JourneyMove> pokemon in Journeys.GroupBy(j => j.PokemonId))
        {
            bool cameToHub = false;

            foreach (JourneyMove move in pokemon.OrderBy(j => j.OccurredOnUtc))
            {
                if (move.Kind == JourneyKind.MovedToHub)
                {
                    cameToHub = true;
                }
                else if (move.Kind == JourneyKind.MovedToSave && cameToHub)
                {
                    returns++;
                    cameToHub = false;
                }
            }
        }

        return returns;
    }
}
