using System.Buffers.Binary;
using System.Security.Cryptography;

namespace PokeHub.Modules.Games.Domain.SaveFormats.Gen3;

/// <summary>A move a new Pokémon knows, with its PP.</summary>
public sealed record Pk3Move(int Id, int Pp);

/// <summary>What PokeHub decides about a Pokémon it creates for a Gen 3 game.</summary>
public sealed record NewPk3
{
    public required uint Personality { get; init; }

    // Trainer id in the low 16 bits, secret id in the high 16 bits.
    public required uint OriginalTrainerId { get; init; }

    // As the game stores it (from a save), or text to encode.
    public required ReadOnlyMemory<byte> OriginalTrainerName { get; init; }

    public required bool OriginalTrainerIsFemale { get; init; }

    public required string Nickname { get; init; }

    public required int Gen3Species { get; init; }

    public required uint Experience { get; init; }

    public required int Friendship { get; init; }

    public required IReadOnlyList<Pk3Move> Moves { get; init; }

    // HP, Attack, Defense, Speed, Sp. Atk, Sp. Def; 0 to 31 each.
    public required IReadOnlyList<int> Ivs { get; init; }

    public required int MetLevel { get; init; }

    public required int MetLocation { get; init; }

    // The game that caught it, as Gen 3 numbers them (4 FireRed, 5 LeafGreen).
    public required int OriginGame { get; init; }
}

/// <summary>
/// A Gen 3 Pokémon as the game stores it in a PC box (80 bytes, "pk3"), following the publicly
/// documented layout. Mirrors pokemon-web's <c>src/lib/gen3/pk3.ts</c> and the decoding in
/// <c>save-reader.ts</c>.
/// <code>
///   0x00 personality value      0x04 original trainer id (trainer id + secret id)
///   0x08 nickname (10)          0x12 language       0x13 flags
///   0x14 original trainer name (7)                  0x1B markings
///   0x1C checksum of the data   0x20 data: four 12-byte blocks, encrypted, ordered by the
///                                    personality value (Growth, Attacks, EVs, Misc)
/// </code>
/// </summary>
public static class Pk3
{
    public const int Size = 80;
    public const int DataOffset = 0x20;
    public const int DataSize = 48;
    public const int BlockSize = 12;

    private const int NicknameOffset = 0x08;
    private const int NicknameSize = 10;
    private const int LanguageOffset = 0x12;
    private const int FlagsOffset = 0x13;
    private const int OtNameOffset = 0x14;
    private const int OtNameSize = 7;
    private const int ChecksumOffset = 0x1C;

    private const byte English = 2;
    // Without this flag the game shows the Pokémon as a Bad Egg.
    private const byte HasSpecies = 0x02;
    private const int PokeBall = 4;

    private static readonly string[] BlockOrders =
    [
        "GAEM", "GAME", "GEAM", "GEMA", "GMAE", "GMEA",
        "AGEM", "AGME", "AEGM", "AEMG", "AMGE", "AMEG",
        "EGAM", "EGMA", "EAGM", "EAMG", "EMGA", "EMAG",
        "MGAE", "MGEA", "MAGE", "MAEG", "MEGA", "MEAG",
    ];

    /// <summary>Where each block sits in the 48 data bytes, for this personality value.</summary>
    public static (int Growth, int Attacks, int Evs, int Misc) Blocks(uint personality)
    {
        string order = BlockOrders[personality % 24];
        return (order.IndexOf('G', StringComparison.Ordinal) * BlockSize,
            order.IndexOf('A', StringComparison.Ordinal) * BlockSize,
            order.IndexOf('E', StringComparison.Ordinal) * BlockSize,
            order.IndexOf('M', StringComparison.Ordinal) * BlockSize);
    }

    /// <summary>How PokeHub knows a Gen 3 Pokémon: its personality value and original trainer id.</summary>
    public static string CatchKey(uint personality, uint originalTrainerId) =>
        $"gen3-{personality:x8}-{originalTrainerId:x8}";

    public static bool IsEmpty(ReadOnlySpan<byte> pokemon) =>
        BinaryPrimitives.ReadUInt32LittleEndian(pokemon) == 0 &&
        BinaryPrimitives.ReadUInt32LittleEndian(pokemon[4..]) == 0;

    /// <summary>
    /// The 48 data bytes decrypted (each 32-bit word XOR personality XOR trainer id), or null when
    /// their checksum doesn't match, which means the Pokémon is corrupted.
    /// </summary>
    public static byte[]? Decrypt(ReadOnlySpan<byte> pokemon)
    {
        uint key = BinaryPrimitives.ReadUInt32LittleEndian(pokemon) ^ BinaryPrimitives.ReadUInt32LittleEndian(pokemon[4..]);
        byte[] data = new byte[DataSize];

        for (int i = 0; i < DataSize; i += 4)
        {
            uint word = BinaryPrimitives.ReadUInt32LittleEndian(pokemon[(DataOffset + i)..]) ^ key;
            BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(i), word);
        }

        return Checksum(data) == BinaryPrimitives.ReadUInt16LittleEndian(pokemon[ChecksumOffset..]) ? data : null;
    }

    public static byte[] Encode(NewPk3 pokemon)
    {
        ArgumentNullException.ThrowIfNull(pokemon);

        byte[] bytes = new byte[Size];
        BinaryPrimitives.WriteUInt32LittleEndian(bytes, pokemon.Personality);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(4), pokemon.OriginalTrainerId);
        Gen3Text.Encode(pokemon.Nickname, NicknameSize).CopyTo(bytes, NicknameOffset);
        bytes[LanguageOffset] = English;
        bytes[FlagsOffset] = HasSpecies;
        pokemon.OriginalTrainerName.Span[..Math.Min(OtNameSize, pokemon.OriginalTrainerName.Length)].CopyTo(bytes.AsSpan(OtNameOffset));

        byte[] data = new byte[DataSize];
        (int growth, int attacks, _, int misc) = Blocks(pokemon.Personality);
        // EVs and contest stats stay 0, as for a newly met Pokémon.

        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(growth), (ushort)pokemon.Gen3Species);
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(growth + 4), pokemon.Experience);
        data[growth + 9] = (byte)pokemon.Friendship;

        for (int i = 0; i < Math.Min(4, pokemon.Moves.Count); i++)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(attacks + i * 2), (ushort)pokemon.Moves[i].Id);
            data[attacks + 8 + i] = (byte)pokemon.Moves[i].Pp;
        }

        data[misc + 1] = (byte)pokemon.MetLocation;
        int origins = (pokemon.MetLevel & 0x7F) |
            ((pokemon.OriginGame & 0xF) << 7) |
            (PokeBall << 11) |
            ((pokemon.OriginalTrainerIsFemale ? 1 : 0) << 15);
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(misc + 2), (ushort)origins);

        // IVs take 5 bits each; bit 30 (egg) and 31 (second ability) stay 0.
        uint ivs = 0;
        for (int i = 0; i < pokemon.Ivs.Count; i++)
        {
            ivs |= (uint)(pokemon.Ivs[i] & 0x1F) << (i * 5);
        }

        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(misc + 4), ivs);

        return Seal(bytes, data);
    }

    /// <summary>
    /// A random personality value, shiny or not for the original trainer: shiny when trainer id
    /// XOR secret id XOR both halves of the personality is below 8 (1 in 8192 at random).
    /// </summary>
    public static uint PersonalityFor(uint originalTrainerId, bool isShiny)
    {
        uint trainerHalves = (originalTrainerId & 0xFFFF) ^ (originalTrainerId >> 16);

        if (isShiny)
        {
            uint high = RandomUInt32() >> 16;
            uint low = (trainerHalves ^ high ^ (RandomUInt32() & 0x7)) & 0xFFFF;
            return (high << 16) | low;
        }

        uint personality;
        do
        {
            personality = RandomUInt32();
        }
        while ((trainerHalves ^ (personality & 0xFFFF) ^ (personality >> 16)) < 8);

        return personality;
    }

    public static int[] RandomIvs() => [.. Enumerable.Range(0, 6).Select(_ => RandomNumberGenerator.GetInt32(32))];

    // The 16-bit sum of the plain data, read as 16-bit words.
    private static ushort Checksum(ReadOnlySpan<byte> data)
    {
        int sum = 0;
        for (int i = 0; i < DataSize; i += 2)
        {
            sum = (sum + BinaryPrimitives.ReadUInt16LittleEndian(data[i..])) & 0xFFFF;
        }

        return (ushort)sum;
    }

    // Stores the checksum of the plain data, then encrypts it.
    private static byte[] Seal(byte[] bytes, byte[] data)
    {
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(ChecksumOffset), Checksum(data));

        uint key = BinaryPrimitives.ReadUInt32LittleEndian(bytes) ^ BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(4));
        for (int i = 0; i < DataSize; i += 4)
        {
            uint word = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(i)) ^ key;
            BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(DataOffset + i), word);
        }

        return bytes;
    }

    private static uint RandomUInt32()
    {
        Span<byte> bytes = stackalloc byte[4];
        RandomNumberGenerator.Fill(bytes);
        return BinaryPrimitives.ReadUInt32LittleEndian(bytes);
    }
}
