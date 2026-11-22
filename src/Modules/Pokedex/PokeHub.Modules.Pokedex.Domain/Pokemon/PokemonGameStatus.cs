namespace PokeHub.Modules.Pokedex.Domain.Pokemon;

/// <summary>
/// How a Pokémon was in one game the last time that game reported it. Kept per game, so what
/// it was like in a game stays after it moves to another.
/// </summary>
public sealed class PokemonGameStatus
{
    private PokemonGameStatus()
    {
    }

    public Guid PokemonId { get; private set; }

    public Guid GameId { get; private set; }

    public int Level { get; private set; }

    // Moves, nature, IVs, EVs and so on, as JSON.
    public string BattleData { get; private set; }

    public DateTime UpdatedOnUtc { get; private set; }

    public static PokemonGameStatus Create(Guid pokemonId, Guid gameId, int level, string battleData, DateTime utcNow) => new()
    {
        PokemonId = pokemonId,
        GameId = gameId,
        Level = level,
        BattleData = battleData,
        UpdatedOnUtc = utcNow,
    };

    public void Update(int level, string battleData, DateTime utcNow)
    {
        if (Level == level && BattleData == battleData)
        {
            return;
        }

        Level = level;
        BattleData = battleData;
        UpdatedOnUtc = utcNow;
    }
}
