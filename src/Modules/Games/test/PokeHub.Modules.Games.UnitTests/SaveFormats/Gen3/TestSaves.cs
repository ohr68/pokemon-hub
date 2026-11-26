using System.Buffers.Binary;
using PokeHub.Modules.Games.Domain.SaveFormats.Gen3;

namespace PokeHub.Modules.Games.UnitTests.SaveFormats.Gen3;

// Synthetic saves built from the documented format, like pokemon-web's tests: no real save is
// ever committed. A real save in the game is the final test.
internal static class TestSaves
{
    public const uint TrainerId = (54321u << 16) | 12345u;

    // The newer copy (B) is stored with its sections rotated, so the reader has to follow the ids.
    public const int NewerTrainerSection = 0xE000 + 5 * 0x1000;
    public const int NewerTeamSection = 0xE000 + 6 * 0x1000;

    /// <summary>An empty save: both copies valid, the newer one (B) rotated.</summary>
    public static byte[] Empty(uint gameCode = 1, uint securityKey = 0)
    {
        byte[] file = new byte[0x20000];

        foreach ((int slotBase, uint saveIndex, int rotation) in new[] { (0, 1u, 0), (0xE000, 2u, 5) })
        {
            for (int id = 0; id < 14; id++)
            {
                int offset = slotBase + (id + rotation) % 14 * 0x1000;
                BinaryPrimitives.WriteUInt16LittleEndian(file.AsSpan(offset + 0xFF4), (ushort)id);
                BinaryPrimitives.WriteUInt32LittleEndian(file.AsSpan(offset + 0xFF8), 0x08012025);
                BinaryPrimitives.WriteUInt32LittleEndian(file.AsSpan(offset + 0xFFC), saveIndex);
            }
        }

        Gen3Text.Encode("RED", 8).CopyTo(file, NewerTrainerSection);
        BinaryPrimitives.WriteUInt32LittleEndian(file.AsSpan(NewerTrainerSection + 0x0A), TrainerId);
        BinaryPrimitives.WriteUInt32LittleEndian(file.AsSpan(NewerTrainerSection + 0xAC), gameCode);
        if (securityKey != 0)
        {
            // FireRed/LeafGreen keep the key at 0xF20; an empty bag slot then holds the key itself.
            BinaryPrimitives.WriteUInt32LittleEndian(file.AsSpan(NewerTrainerSection + 0xF20), securityKey);
        }

        return file;
    }

    public static byte[] Charmander(uint personality) => Pk3.Encode(new NewPk3
    {
        Personality = personality,
        OriginalTrainerId = TrainerId,
        OriginalTrainerName = Gen3Text.Encode("RED", 7),
        OriginalTrainerIsFemale = false,
        Nickname = "CHARMANDER",
        Gen3Species = 4,
        Experience = 135,
        Friendship = 70,
        Moves = [new Pk3Move(10, 35), new Pk3Move(45, 40)],
        Ivs = [31, 0, 15, 7, 20, 1],
        MetLevel = 5,
        MetLocation = 0x58,
        OriginGame = 4,
    });
}
