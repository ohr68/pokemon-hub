namespace PokeHub.Modules.Pokedex.PublicApi;

/// <summary>What a game knows about one of its Pokémon, by catch key.</summary>
/// <param name="Level">Its level in that game now.</param>
public sealed record PokemonBattleReport(string CatchKey, int Level, PokemonBattleData Data);

/// <param name="Nature">0 to 24, in the games' order (Hardy to Quirky).</param>
/// <param name="AbilitySlot">Which of the species' abilities: 0 or 1.</param>
/// <param name="HeldItemId">The game's item number; 0 for none.</param>
/// <param name="HeldItemNumbering">
/// Which games' item numbers <paramref name="HeldItemId"/> is: "gen3" for Ruby to LeafGreen,
/// "modern" for X to Ultra Moon.
/// </param>
/// <param name="Ivs">HP, Attack, Defense, Speed, Sp. Atk, Sp. Def.</param>
/// <param name="Evs">In the same order as the IVs.</param>
/// <param name="Stats">Only for Pokémon in the party: games don't store them for the PC.</param>
public sealed record PokemonBattleData(
    int Nature,
    int AbilitySlot,
    int HeldItemId,
    string HeldItemNumbering,
    int Friendship,
    IReadOnlyList<PokemonBattleMove> Moves,
    IReadOnlyList<int> Ivs,
    IReadOnlyList<int> Evs,
    PokemonBattleStats? Stats);

public sealed record PokemonBattleMove(int Id, int Pp, int PpUps);

public sealed record PokemonBattleStats(int CurrentHp, int MaxHp, int Attack, int Defense, int Speed, int SpecialAttack, int SpecialDefense);
