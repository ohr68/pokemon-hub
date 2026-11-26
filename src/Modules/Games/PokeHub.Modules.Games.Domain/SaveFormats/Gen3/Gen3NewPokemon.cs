namespace PokeHub.Modules.Games.Domain.SaveFormats.Gen3;

/// <summary>A species as a Gen 3 game knows it, with the FireRed/LeafGreen learnset.</summary>
/// <param name="Name">PokéAPI's name, such as mr-mime.</param>
/// <param name="ExperienceByLevel">Experience needed for each level (index 0 is level 1).</param>
/// <param name="LevelUpMoves">In the order the species learns them.</param>
public sealed record Gen3Species(
    int Gen3Index,
    string Name,
    IReadOnlyList<int> ExperienceByLevel,
    IReadOnlyList<(int Level, int MoveId)> LevelUpMoves);

/// <summary>A Pokémon from the hub that has never been in a Gen 3 game.</summary>
/// <param name="OriginalTrainerKey">Identifies its original PokeHub trainer.</param>
public sealed record HubPokemon(
    string? Nickname,
    int Level,
    bool IsShiny,
    bool IsOriginalTrainer,
    string OriginalTrainerKey,
    string OriginalTrainerName);

/// <summary>
/// Creates a Gen 3 Pokémon for a hub Pokémon that was never in one. Mirrors
/// pokemon-web's <c>newPk3</c> in <c>src/lib/gen3/box-transfer.ts</c>: the moves it would know
/// at its level, random IVs and nature, met in this game, and the player as original trainer
/// when they are, or a trainer of its own when it was traded.
/// </summary>
public static class Gen3NewPokemon
{
    // A Pokémon's starting friendship in most species; the catalog doesn't carry it yet.
    public const int BaseFriendship = 70;
    // Gen 3 games know moves up to #354.
    public const int LastGen3Move = 354;

    private static readonly Pk3Move Tackle = new(33, 35);

    // The game names a Pokémon without a nickname after its species, in capitals.
    private static readonly Dictionary<string, string> SpecialNames = new(StringComparer.Ordinal)
    {
        ["nidoran-f"] = "NIDORAN♀",
        ["nidoran-m"] = "NIDORAN♂",
        ["mr-mime"] = "MR. MIME",
        ["farfetchd"] = "FARFETCH'D",
        ["ho-oh"] = "HO-OH",
    };

    public static byte[] Create(HubPokemon pokemon, Gen3Species species, Gen3Save save, Gen3Game game, IReadOnlyDictionary<int, int> movePp)
    {
        ArgumentNullException.ThrowIfNull(pokemon);
        ArgumentNullException.ThrowIfNull(species);
        ArgumentNullException.ThrowIfNull(save);
        ArgumentNullException.ThrowIfNull(game);

        uint originalTrainerId = pokemon.IsOriginalTrainer
            ? save.TrainerId
            : ForeignTrainerId(pokemon.OriginalTrainerKey, save.TrainerId);

        return Pk3.Encode(new NewPk3
        {
            Personality = Pk3.PersonalityFor(originalTrainerId, pokemon.IsShiny),
            OriginalTrainerId = originalTrainerId,
            OriginalTrainerName = pokemon.IsOriginalTrainer
                ? save.PlayerName
                : Gen3Text.Encode(pokemon.OriginalTrainerName.ToUpperInvariant(), 7),
            OriginalTrainerIsFemale = pokemon.IsOriginalTrainer && save.PlayerIsFemale,
            Nickname = pokemon.Nickname ?? DisplayName(species.Name),
            Gen3Species = species.Gen3Index,
            Experience = pokemon.Level >= 1 && pokemon.Level <= species.ExperienceByLevel.Count
                ? (uint)species.ExperienceByLevel[pokemon.Level - 1]
                : 0,
            Friendship = BaseFriendship,
            Moves = MovesAtLevel(species, pokemon.Level, movePp),
            Ivs = Pk3.RandomIvs(),
            MetLevel = pokemon.Level,
            // Met in this game, in a town of its region, so syncing it here counts it as this game's.
            MetLocation = game.MetLocation,
            OriginGame = game.OriginGame,
        });
    }

    /// <summary>The last four moves the species learned by this level, like a wild Pokémon of that level.</summary>
    public static IReadOnlyList<Pk3Move> MovesAtLevel(Gen3Species species, int level, IReadOnlyDictionary<int, int> movePp)
    {
        ArgumentNullException.ThrowIfNull(species);
        ArgumentNullException.ThrowIfNull(movePp);

        var known = new List<int>();
        foreach ((int learnedAt, int moveId) in species.LevelUpMoves)
        {
            if (learnedAt <= level && moveId <= LastGen3Move && !known.Contains(moveId))
            {
                known.Add(moveId);
            }
        }

        List<Pk3Move> moves = [.. known.TakeLast(4).Select(id => new Pk3Move(id, movePp.GetValueOrDefault(id)))];
        return moves.Count > 0 ? moves : [Tackle];
    }

    public static string DisplayName(string speciesName)
    {
        ArgumentNullException.ThrowIfNull(speciesName);

        return SpecialNames.TryGetValue(speciesName, out string? special)
            ? special
            : speciesName.Replace('-', ' ').ToUpperInvariant();
    }

    /// <summary>
    /// The Pokémon's nickname, or null when it carries its species name (BULBASAUR), which isn't a
    /// real nickname. Compared on letters alone, so NIDORAN♀ matches nidoran-f.
    /// </summary>
    public static string? RealNickname(string nickname, string speciesName)
    {
        ArgumentNullException.ThrowIfNull(nickname);
        ArgumentNullException.ThrowIfNull(speciesName);

        static string Letters(string text) => new([.. text.ToUpperInvariant().Where(c => c is >= 'A' and <= 'Z')]);

        string letters = Letters(nickname);
        return letters.Length == 0 || Letters(speciesName).StartsWith(letters, StringComparison.Ordinal) ? null : nickname;
    }

    /// <summary>Box Pokémon only store experience: the level is the highest one whose threshold it reached.</summary>
    public static int LevelFromExperience(uint experience, IReadOnlyList<int> experienceByLevel)
    {
        ArgumentNullException.ThrowIfNull(experienceByLevel);

        int level = 1;
        for (int i = 0; i < experienceByLevel.Count; i++)
        {
            if (experience >= experienceByLevel[i])
            {
                level = i + 1;
            }
        }

        return level;
    }

    /// <summary>The same PokeHub trainer always gets the same trainer and secret id, never the player's.</summary>
    public static uint ForeignTrainerId(string pokehubTrainerId, uint playerTrainerId)
    {
        ArgumentNullException.ThrowIfNull(pokehubTrainerId);

        // FNV-1a over the text's UTF-16 units, like pokemon-web's charCodeAt loop.
        uint hash = 0x811C9DC5;
        foreach (char c in pokehubTrainerId)
        {
            hash = unchecked((hash ^ c) * 0x01000193);
        }

        return hash == playerTrainerId ? hash ^ 1 : hash;
    }
}
