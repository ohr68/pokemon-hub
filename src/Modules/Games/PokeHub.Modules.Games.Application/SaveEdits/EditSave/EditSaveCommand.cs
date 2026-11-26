using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Games.Application.SaveEdits.EditSave;

/// <summary>
/// A device that plays the game elsewhere (the 3DS app) sends the game's save; PokeHub backs it
/// up, puts in the Pokémon waiting for the game and hands the edited save back for the device
/// to write.
/// </summary>
/// <param name="ClientId">The client the player signed in with, from the token.</param>
/// <param name="Content">The save file exactly as the game wrote it.</param>
/// <param name="Label">Where it came from, kept on its backup, such as "Do 3DS".</param>
/// <param name="Withdraw">Catch keys of PC box Pokémon to take out of the save, to the hub.</param>
public sealed record EditSaveCommand(
    string? ClientId,
    Guid TrainerId,
    byte[] Content,
    string? Label = null,
    IReadOnlyCollection<string>? Withdraw = null)
    : ICommand<EditedSaveResponse>;

/// <param name="Content">The save to write into the game; the same bytes when nothing changed.</param>
/// <param name="Changed">False when there was nothing to put in, so the device can skip writing.</param>
/// <param name="EditId">The edit, to report with save/edits/{id}/unwritten if the device can't write it; null when nothing changed.</param>
/// <param name="Withdrawn">Pokémon taken out of the save; they're in the hub now.</param>
/// <param name="Items">Shop items put into the bag.</param>
/// <param name="ItemsLeft">Shop items that didn't fit; they keep waiting.</param>
/// <param name="Settled">Pokémon of an earlier edit the game never got, put back where its save says they are.</param>
public sealed record EditedSaveResponse(
    Guid? EditId,
    byte[] Content,
    bool Changed,
    IReadOnlyCollection<DeliveredToSaveResponse> Delivered,
    IReadOnlyCollection<NotDeliveredResponse> NotDelivered,
    IReadOnlyCollection<WithdrawnFromSaveResponse> Withdrawn,
    IReadOnlyCollection<ItemToSaveResponse> Items,
    IReadOnlyCollection<ItemToSaveResponse> ItemsLeft,
    int Settled);

/// <param name="Reason">Why it stayed out, for one that didn't fit.</param>
public sealed record ItemToSaveResponse(Guid PurchaseId, string Name, int Quantity, string? Reason = null);

public sealed record WithdrawnFromSaveResponse(Guid PokemonId, string Name, int SpeciesId, bool IsShiny);

/// <param name="Box">The PC box, as the game shows it (from 1).</param>
/// <param name="Slot">The slot in that box (from 1).</param>
public sealed record DeliveredToSaveResponse(Guid PokemonId, string Name, int Box, int Slot, int SpeciesId, bool IsShiny);

public sealed record NotDeliveredResponse(Guid PokemonId, string Name, string Reason, int SpeciesId = 0, bool IsShiny = false);
