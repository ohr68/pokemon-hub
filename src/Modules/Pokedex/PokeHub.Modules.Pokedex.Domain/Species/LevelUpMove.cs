namespace PokeHub.Modules.Pokedex.Domain.Species;

/// <summary>
/// A move a species learns on reaching a level. <see cref="Order"/> keeps the game's order
/// for moves learned at the same level.
/// </summary>
public sealed record LevelUpMove(int Order, int Level, int MoveId);
