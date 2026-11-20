namespace PokeHub.Modules.Games.Presentation.Play.Contracts;

internal sealed record DeliverPokemonRequest(IReadOnlyCollection<DeliveryItem> Deliveries);

/// <param name="CatchKey">How the game knows the Pokémon now that it is in its save.</param>
internal sealed record DeliveryItem(Guid PokemonId, string CatchKey);

internal sealed record WithdrawPokemonRequest(IReadOnlyCollection<WithdrawalItem> Withdrawals);

/// <param name="SpeciesId">National number; the Pokémon may have evolved in the game.</param>
/// <param name="DataFormat">The format of <paramref name="Data"/>, such as pk3.</param>
/// <param name="Data">The game's own bytes for the Pokémon, base64 in JSON.</param>
internal sealed record WithdrawalItem(
    string CatchKey,
    int SpeciesId,
    int Level,
    string? Nickname,
    string DataFormat,
    byte[] Data);
