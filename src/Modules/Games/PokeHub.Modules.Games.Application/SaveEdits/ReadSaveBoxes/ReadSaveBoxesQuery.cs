using PokeHub.Common.Application.Messaging;

namespace PokeHub.Modules.Games.Application.SaveEdits.ReadSaveBoxes;

/// <summary>
/// The Pokémon in a save's PC boxes, read by PokeHub for a device that can't read the save
/// itself (the 3DS app), to pick ones to send to the hub. Changes nothing.
/// </summary>
/// <param name="ClientId">The client the player signed in with, from the token.</param>
/// <param name="Content">The save file exactly as the game wrote it.</param>
public sealed record ReadSaveBoxesQuery(string? ClientId, byte[] Content) : IQuery<SaveBoxesResponse>;

public sealed record SaveBoxesResponse(IReadOnlyCollection<BoxPokemonResponse> Pokemon);

/// <param name="Box">The PC box, as the game shows it (from 1).</param>
/// <param name="Slot">The slot in that box (from 1).</param>
/// <param name="Name">As the game shows it: its nickname, or its species in capitals.</param>
/// <param name="CatchKey">How PokeHub knows it; send it back to take it out of the save.</param>
public sealed record BoxPokemonResponse(
    int Box,
    int Slot,
    int SpeciesId,
    string Name,
    int Level,
    bool IsShiny,
    string CatchKey);
