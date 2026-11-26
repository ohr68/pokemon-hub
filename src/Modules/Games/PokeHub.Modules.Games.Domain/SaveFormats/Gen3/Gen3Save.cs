using System.Buffers.Binary;

namespace PokeHub.Modules.Games.Domain.SaveFormats.Gen3;

public enum Gen3Location
{
    Party,
    Box,
}

/// <summary>A Pokémon in a Gen 3 save, as PokeHub needs it to move Pokémon in and out.</summary>
public sealed record Gen3Pokemon(
    uint Personality,
    uint OriginalTrainerId,
    // The species' number inside Gen 3 games (not the national number after #251).
    int Gen3Species,
    uint Experience,
    string Nickname,
    bool IsShiny,
    // The game that caught it, as Gen 3 numbers them (4 FireRed, 5 LeafGreen, 1 to 3 Ruby,
    // Sapphire and Emerald).
    int OriginGame,
    Gen3Location Location,
    // Where it sits in the PC (0 is box 1, slot 1), for box Pokémon.
    int? BoxSlot,
    // Its 80 bytes as the game stores them in a box (encrypted).
    ReadOnlyMemory<byte> Raw)
{
    public string CatchKey => Pk3.CatchKey(Personality, OriginalTrainerId);
}

/// <summary>An item to put into the bag.</summary>
public sealed record Gen3BagItem(int ItemId, Gen3Pocket Pocket, int Quantity);

/// <summary>Why an item stayed out of the bag.</summary>
public enum Gen3BagRefusal
{
    None,
    PocketFull,
    StackFull,
}

/// <summary>
/// A Generation III save (FireRed/LeafGreen, Ruby/Sapphire or Emerald), read and changed in
/// place, following the publicly documented layout. Mirrors pokemon-web's
/// <c>src/lib/gen3/save-reader.ts</c>, <c>save-writer.ts</c> and <c>bag.ts</c>.
/// <para>
/// The 128 KB save holds two copies of the game (slots A and B); the game writes them
/// alternately, so the copy with the higher save index is the latest. Each copy is 14 sections
/// of 4 KB in a rotating order, identified by a footer. Only the latest copy is changed, and
/// every section touched gets its checksum recomputed, or the game would reject the save.
/// </para>
/// </summary>
public sealed class Gen3Save
{
    public const int PcCapacity = 14 * 30;
    // More than this in one stack isn't something every Gen 3 game can show.
    public const int MaxStack = 99;

    private const int SectionSize = 0x1000;
    private const int SectionDataSize = 0xF80;
    private const int SectionsPerSlot = 14;
    private const int SlotBOffset = 0xE000;
    private const uint SectionSignature = 0x08012025;
    private const int FooterId = 0xFF4;
    private const int FooterChecksum = 0xFF6;
    private const int FooterSignature = 0xFF8;
    private const int FooterSaveIndex = 0xFFC;

    // Section 0: trainer info. Section 1: party and bag. Sections 5–13: PC boxes.
    private const int TrainerSection = 0;
    private const int TeamSection = 1;
    private const int FirstPcSection = 5;
    private const int LastPcSection = 13;
    private const int LastPcSectionDataSize = 2000;
    private const int PcPokemonOffset = 4;

    private const int PlayerNameOffset = 0x00;
    private const int PlayerNameSize = 8;
    private const int PlayerGenderOffset = 0x08;
    private const int TrainerIdOffset = 0x0A;
    private const int PartyPokemonSize = 100;
    private const int BagSlotSize = 4;

    private readonly byte[] _file;
    // Section id → byte offset of that section in the file, for the latest copy.
    private readonly Dictionary<int, int> _sections;
    private readonly byte[] _pc;

    private Gen3Save(byte[] file, Dictionary<int, int> sections, Gen3Layout layout)
    {
        _file = file;
        _sections = sections;
        Layout = layout;
        _pc = ReadPc();
    }

    public Gen3Layout Layout { get; }

    /// <summary>Trainer id (low 16 bits) and secret id (high 16 bits), as the game stores them.</summary>
    public uint TrainerId => BinaryPrimitives.ReadUInt32LittleEndian(_file.AsSpan(_sections[TrainerSection] + TrainerIdOffset));

    /// <summary>The player's name as the game stores it (8 bytes, ended with 0xFF).</summary>
    public ReadOnlyMemory<byte> PlayerName => _file.AsMemory(_sections[TrainerSection] + PlayerNameOffset, PlayerNameSize);

    public bool PlayerIsFemale => _file[_sections[TrainerSection] + PlayerGenderOffset] == 1;

    /// <summary>Reads a save; the family of the game it's for, when known, settles an Emerald save's key.</summary>
    public static Gen3Save Read(ReadOnlySpan<byte> file, Gen3Family? family = null)
    {
        byte[] copy = file.ToArray();
        Dictionary<int, int> sections = LatestSlot(copy) ??
            throw new Gen3SaveException("No valid save found. Save the game in the game first.");

        uint gameCode = BinaryPrimitives.ReadUInt32LittleEndian(copy.AsSpan(sections[TrainerSection] + Gen3Layout.GameCodeOffset));
        return new Gen3Save(copy, sections, Gen3Layout.For(gameCode, family));
    }

    /// <summary>The party and PC box Pokémon; eggs and corrupted ones are left out.</summary>
    public IReadOnlyList<Gen3Pokemon> Pokemon()
    {
        var pokemon = new List<Gen3Pokemon>();
        int team = _sections[TeamSection];
        int teamSize = (int)Math.Min(BinaryPrimitives.ReadUInt32LittleEndian(_file.AsSpan(team + Layout.TeamSizeOffset)), 6);

        for (int i = 0; i < teamSize; i++)
        {
            AddIfReadable(pokemon, _file.AsMemory(team + Layout.TeamOffset + i * PartyPokemonSize, Pk3.Size), Gen3Location.Party, null);
        }

        for (int i = 0; i < PcCapacity; i++)
        {
            AddIfReadable(pokemon, _pc.AsMemory(PcPokemonOffset + i * Pk3.Size, Pk3.Size), Gen3Location.Box, i);
        }

        return pokemon;
    }

    /// <summary>PC positions without a Pokémon, from box 1, slot 1.</summary>
    public IReadOnlyList<int> FreeSlots() =>
        [.. Enumerable.Range(0, PcCapacity).Where(i => Pk3.IsEmpty(_pc.AsSpan(PcPokemonOffset + i * Pk3.Size, Pk3.Size)))];

    /// <summary>Puts a Pokémon (80 bytes, as stored in a box) at a PC position, or empties it.</summary>
    public void SetSlot(int index, ReadOnlySpan<byte> pokemon)
    {
        if (index is < 0 or >= PcCapacity)
        {
            throw new Gen3SaveException($"There is no PC position {index}.");
        }

        if (!pokemon.IsEmpty && pokemon.Length != Pk3.Size)
        {
            throw new Gen3SaveException($"A Pokémon in a box takes {Pk3.Size} bytes, not {pokemon.Length}.");
        }

        Span<byte> slot = _pc.AsSpan(PcPokemonOffset + index * Pk3.Size, Pk3.Size);
        if (pokemon.IsEmpty)
        {
            slot.Clear();
        }
        else
        {
            pokemon.CopyTo(slot);
        }

        WritePc();
    }

    /// <summary>Items already in a pocket, as the game would show them.</summary>
    public IReadOnlyList<(int ItemId, int Quantity)> ReadPocket(Gen3Pocket pocket)
    {
        Gen3PocketLayout layout = Layout.Pocket(pocket);
        int section = _sections[TeamSection];
        ushort key = BagKey();
        var items = new List<(int, int)>();

        for (int i = 0; i < layout.Slots; i++)
        {
            int at = section + layout.Offset + i * BagSlotSize;
            ushort itemId = BinaryPrimitives.ReadUInt16LittleEndian(_file.AsSpan(at));
            if (itemId != 0)
            {
                items.Add((itemId, BinaryPrimitives.ReadUInt16LittleEndian(_file.AsSpan(at + 2)) ^ key));
            }
        }

        return items;
    }

    /// <summary>Adds to the item's stack, or to the first empty slot of its pocket.</summary>
    public Gen3BagRefusal AddItem(Gen3BagItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        Gen3PocketLayout pocket = Layout.Pocket(item.Pocket);
        int section = _sections[TeamSection];
        int stack = -1;
        int empty = -1;

        for (int i = 0; i < pocket.Slots; i++)
        {
            ushort id = BinaryPrimitives.ReadUInt16LittleEndian(_file.AsSpan(section + pocket.Offset + i * BagSlotSize));
            if (id == item.ItemId && stack < 0)
            {
                stack = i;
            }

            if (id == 0 && empty < 0)
            {
                empty = i;
            }
        }

        int index = stack >= 0 ? stack : empty;
        if (index < 0)
        {
            return Gen3BagRefusal.PocketFull;
        }

        int at = section + pocket.Offset + index * BagSlotSize;
        ushort key = BagKey();
        int current = stack >= 0 ? BinaryPrimitives.ReadUInt16LittleEndian(_file.AsSpan(at + 2)) ^ key : 0;

        if (current + item.Quantity > MaxStack)
        {
            return Gen3BagRefusal.StackFull;
        }

        BinaryPrimitives.WriteUInt16LittleEndian(_file.AsSpan(at), (ushort)item.ItemId);
        BinaryPrimitives.WriteUInt16LittleEndian(_file.AsSpan(at + 2), (ushort)((current + item.Quantity) ^ key));
        WriteChecksum(section, SectionDataSize);
        return Gen3BagRefusal.None;
    }

    /// <summary>The save with every change made so far.</summary>
    public byte[] ToArray() => (byte[])_file.Clone();

    /// <summary>Box and slot as the game shows them (1-based), for a PC position.</summary>
    public static (int Box, int Slot) BoxPosition(int index) => (index / 30 + 1, index % 30 + 1);

    /// <summary>The 32-bit sum of the section's data, read as 32-bit words, folded into 16 bits.</summary>
    public static ushort SectionChecksum(ReadOnlySpan<byte> data)
    {
        uint sum = 0;
        for (int i = 0; i < data.Length; i += 4)
        {
            sum += BinaryPrimitives.ReadUInt32LittleEndian(data[i..]);
        }

        return (ushort)((sum >> 16) + (sum & 0xFFFF));
    }

    private static Dictionary<int, int>? LatestSlot(byte[] file)
    {
        (uint Index, Dictionary<int, int> Sections)? latest = null;

        foreach (int offset in new[] { 0, SlotBOffset })
        {
            (uint, Dictionary<int, int>)? slot = ReadSlot(file, offset);
            if (slot is { } found && (latest is null || found.Item1 > latest.Value.Index))
            {
                latest = found;
            }
        }

        return latest?.Sections;
    }

    private static (uint SaveIndex, Dictionary<int, int> Sections)? ReadSlot(byte[] file, int slotOffset)
    {
        if (file.Length < slotOffset + SectionsPerSlot * SectionSize)
        {
            return null;
        }

        var sections = new Dictionary<int, int>();
        uint saveIndex = 0;

        for (int i = 0; i < SectionsPerSlot; i++)
        {
            int offset = slotOffset + i * SectionSize;
            if (BinaryPrimitives.ReadUInt32LittleEndian(file.AsSpan(offset + FooterSignature)) != SectionSignature)
            {
                return null;
            }

            sections[BinaryPrimitives.ReadUInt16LittleEndian(file.AsSpan(offset + FooterId))] = offset;
            saveIndex = BinaryPrimitives.ReadUInt32LittleEndian(file.AsSpan(offset + FooterSaveIndex));
        }

        return sections.Count == SectionsPerSlot ? (saveIndex, sections) : null;
    }

    private static void AddIfReadable(List<Gen3Pokemon> pokemon, ReadOnlyMemory<byte> bytes, Gen3Location location, int? boxSlot)
    {
        ReadOnlySpan<byte> span = bytes.Span;
        if (Pk3.IsEmpty(span))
        {
            return;
        }

        byte[]? data = Pk3.Decrypt(span);
        if (data is null)
        {
            return;
        }

        uint personality = BinaryPrimitives.ReadUInt32LittleEndian(span);
        uint trainerId = BinaryPrimitives.ReadUInt32LittleEndian(span[4..]);
        (int growth, _, _, int misc) = Pk3.Blocks(personality);

        int species = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(growth));
        bool isEgg = ((BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(misc + 4)) >> 30) & 1) == 1;
        if (species == 0 || isEgg)
        {
            return;
        }

        // Shiny when trainer id XOR secret id XOR both halves of the personality is below 8.
        uint shinyValue = (trainerId & 0xFFFF) ^ (trainerId >> 16) ^ (personality & 0xFFFF) ^ (personality >> 16);

        pokemon.Add(new Gen3Pokemon(
            personality,
            trainerId,
            species,
            BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(growth + 4)),
            Gen3Text.Decode(span[0x08..0x12]),
            shinyValue < 8,
            // Bits 7 to 10 of the origins word, after the met level.
            (BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(misc + 2)) >> 7) & 0xF,
            location,
            boxSlot,
            bytes[..Pk3.Size].ToArray()));
    }

    // FireRed/LeafGreen and Emerald XOR item quantities with the security key's low half.
    private ushort BagKey() => Layout.SecurityKeyOffset is int offset
        ? (ushort)(BinaryPrimitives.ReadUInt32LittleEndian(_file.AsSpan(_sections[TrainerSection] + offset)) & 0xFFFF)
        : (ushort)0;

    // The PC data is split across sections 5–13; stitched together it reads as one buffer.
    private byte[] ReadPc()
    {
        using var pc = new MemoryStream();
        for (int id = FirstPcSection; id <= LastPcSection; id++)
        {
            pc.Write(_file, _sections[id], PcSectionSize(id));
        }

        return pc.ToArray();
    }

    private void WritePc()
    {
        int offset = 0;
        for (int id = FirstPcSection; id <= LastPcSection; id++)
        {
            int size = PcSectionSize(id);
            _pc.AsSpan(offset, size).CopyTo(_file.AsSpan(_sections[id]));
            WriteChecksum(_sections[id], size);
            offset += size;
        }
    }

    private void WriteChecksum(int section, int size) =>
        BinaryPrimitives.WriteUInt16LittleEndian(_file.AsSpan(section + FooterChecksum), SectionChecksum(_file.AsSpan(section, size)));

    private static int PcSectionSize(int id) => id == LastPcSection ? LastPcSectionDataSize : SectionDataSize;
}
