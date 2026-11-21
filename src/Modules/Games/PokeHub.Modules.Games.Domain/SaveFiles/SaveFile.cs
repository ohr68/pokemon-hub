using System.Security.Cryptography;
using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Games.Domain.SaveFiles;

/// <summary>
/// A copy of a player's save file, exactly as their game wrote it, kept on PokeHub so the
/// save survives the browser losing it. Each upload is a new backup; the newest one is the
/// player's current save.
/// </summary>
public sealed class SaveFile : Entity
{
    // Older backups are removed once a player has more than this many for a game.
    public const int BackupsKept = 20;

    // Generous: a GBA save is at most 128 KB.
    public const int MaxSizeBytes = 1024 * 1024;

    private SaveFile()
    {
    }

    public Guid Id { get; private set; }

    public Guid TrainerId { get; private set; }

    public Guid GameId { get; private set; }

    public int SizeBytes { get; private set; }

    // Hex SHA-256 of the content: tells an unchanged save apart without comparing bytes.
    public string Sha256 { get; private set; }

    public byte[] Content { get; private set; }

    public DateTime UploadedOnUtc { get; private set; }

    // Where it came from when it wasn't a sync, such as "Importado: firered.sav".
    public string? Label { get; private set; }

    public static string HashOf(byte[] content) => Convert.ToHexStringLower(SHA256.HashData(content));

    public static SaveFile Upload(Guid trainerId, Guid gameId, byte[] content, DateTime utcNow, string? label = null)
    {
        return new SaveFile
        {
            Label = string.IsNullOrWhiteSpace(label) ? null : label.Trim(),
            Id = Guid.CreateVersion7(),
            TrainerId = trainerId,
            GameId = gameId,
            SizeBytes = content.Length,
            Sha256 = HashOf(content),
            Content = content,
            UploadedOnUtc = utcNow,
        };
    }
}
