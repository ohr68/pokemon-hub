using System.Buffers.Binary;
using FluentAssertions;
using PokeHub.Modules.Games.Domain.SaveFormats.Gen3;

namespace PokeHub.Modules.Games.UnitTests.SaveFormats.Gen3;

public class Gen3SaveTests
{
    [Fact]
    public void Read_ShouldTakeTheNewerCopy_AndItsTrainer()
    {
        var save = Gen3Save.Read(TestSaves.Empty());

        save.TrainerId.Should().Be(TestSaves.TrainerId);
        Gen3Text.Decode(save.PlayerName.Span).Should().Be("RED");
        save.Layout.Should().Be(Gen3Layout.FireRedLeafGreen);
        save.Pokemon().Should().BeEmpty();
        save.FreeSlots().Should().HaveCount(Gen3Save.PcCapacity);
    }

    [Theory]
    [InlineData(1u, null, Gen3Family.FireRedLeafGreen)]
    [InlineData(0u, null, Gen3Family.RubySapphire)]
    [InlineData(0x1234ABCDu, null, Gen3Family.Emerald)]
    [InlineData(1u, Gen3Family.Emerald, Gen3Family.Emerald)]
    public void Read_ShouldTellTheFamilyApart_ByGameCode(uint gameCode, Gen3Family? known, Gen3Family expected)
    {
        Gen3Save.Read(TestSaves.Empty(gameCode), known).Layout.Family.Should().Be(expected);
    }

    [Fact]
    public void Read_ShouldRefuse_AFileWithoutAValidCopy()
    {
        Action read = () => Gen3Save.Read(new byte[0x20000]);

        read.Should().Throw<Gen3SaveException>();
    }

    [Fact]
    public void SetSlot_ShouldWritePokemon_ThatReadBackAsWritten()
    {
        var save = Gen3Save.Read(TestSaves.Empty());
        uint shiny = Pk3.PersonalityFor(TestSaves.TrainerId, isShiny: true);
        uint plain = Pk3.PersonalityFor(TestSaves.TrainerId, isShiny: false);

        save.SetSlot(0, TestSaves.Charmander(shiny));
        save.SetSlot(31, TestSaves.Charmander(plain));
        IReadOnlyList<Gen3Pokemon> pokemon = Gen3Save.Read(save.ToArray()).Pokemon();

        pokemon.Should().HaveCount(2);
        pokemon[0].Should().BeEquivalentTo(new
        {
            Gen3Species = 4,
            Experience = 135u,
            Nickname = "CHARMANDER",
            IsShiny = true,
            BoxSlot = 0,
            OriginalTrainerId = TestSaves.TrainerId,
            OriginGame = 4,
            Location = Gen3Location.Box,
        });
        pokemon[1].IsShiny.Should().BeFalse();
        pokemon[1].BoxSlot.Should().Be(31);
        pokemon[1].CatchKey.Should().Be($"gen3-{plain:x8}-{TestSaves.TrainerId:x8}");
        Gen3Save.BoxPosition(31).Should().Be((2, 2));
    }

    [Fact]
    public void SetSlot_ShouldKeepEveryPcChecksumValid_AndLeaveTheOlderCopyAlone()
    {
        byte[] original = TestSaves.Empty();
        var save = Gen3Save.Read(original);

        save.SetSlot(419, TestSaves.Charmander(Pk3.PersonalityFor(TestSaves.TrainerId, isShiny: false)));
        byte[] written = save.ToArray();

        for (int i = 0; i < 14; i++)
        {
            int section = 0xE000 + i * 0x1000;
            int id = BinaryPrimitives.ReadUInt16LittleEndian(written.AsSpan(section + 0xFF4));
            if (id < 5)
            {
                continue;
            }

            int size = id == 13 ? 2000 : 0xF80;
            BinaryPrimitives.ReadUInt16LittleEndian(written.AsSpan(section + 0xFF6))
                .Should().Be(Gen3Save.SectionChecksum(written.AsSpan(section, size)), $"section {id}");
        }

        written.AsSpan(0, 0xE000).SequenceEqual(original.AsSpan(0, 0xE000)).Should().BeTrue();
        Gen3Save.Read(written).FreeSlots().Should().NotContain(419).And.HaveCount(Gen3Save.PcCapacity - 1);
    }

    [Fact]
    public void SetSlot_ShouldEmptyASlot_WhenGivenNothing()
    {
        var save = Gen3Save.Read(TestSaves.Empty());
        save.SetSlot(5, TestSaves.Charmander(Pk3.PersonalityFor(TestSaves.TrainerId, isShiny: false)));

        save.SetSlot(5, []);

        Gen3Save.Read(save.ToArray()).Pokemon().Should().BeEmpty();
    }

    [Fact]
    public void SetSlot_ShouldRefuse_APositionOutsideThePc()
    {
        var save = Gen3Save.Read(TestSaves.Empty());

        Action write = () => save.SetSlot(420, TestSaves.Charmander(1));

        write.Should().Throw<Gen3SaveException>();
    }

    [Fact]
    public void Read_ShouldSkip_ACorruptedPokemon()
    {
        var save = Gen3Save.Read(TestSaves.Empty());
        byte[] broken = TestSaves.Charmander(Pk3.PersonalityFor(TestSaves.TrainerId, isShiny: false));
        broken[0x20] ^= 0xFF;

        save.SetSlot(0, broken);

        Gen3Save.Read(save.ToArray()).Pokemon().Should().BeEmpty();
    }

    [Fact]
    public void AddItem_ShouldStackAndFillThePocket_WithTheSecurityKey()
    {
        const uint key = 0xBEEF1234;
        var save = Gen3Save.Read(TestSaves.Empty(securityKey: key));

        save.AddItem(new Gen3BagItem(13, Gen3Pocket.Items, 5)).Should().Be(Gen3BagRefusal.None);
        save.AddItem(new Gen3BagItem(13, Gen3Pocket.Items, 3)).Should().Be(Gen3BagRefusal.None);
        save.AddItem(new Gen3BagItem(4, Gen3Pocket.Balls, 10)).Should().Be(Gen3BagRefusal.None);
        var read = Gen3Save.Read(save.ToArray());

        read.ReadPocket(Gen3Pocket.Items).Should().Equal((13, 8));
        read.ReadPocket(Gen3Pocket.Balls).Should().Equal((4, 10));

        // Stored XORed with the key's low half.
        int slot = TestSaves.NewerTeamSection + 0x310;
        BinaryPrimitives.ReadUInt16LittleEndian(save.ToArray().AsSpan(slot + 2)).Should().Be((ushort)(8 ^ 0x1234));
        BinaryPrimitives.ReadUInt16LittleEndian(save.ToArray().AsSpan(TestSaves.NewerTeamSection + 0xFF6))
            .Should().Be(Gen3Save.SectionChecksum(save.ToArray().AsSpan(TestSaves.NewerTeamSection, 0xF80)));
    }

    [Fact]
    public void AddItem_ShouldRefuse_AStackPast99_OrAFullPocket()
    {
        var save = Gen3Save.Read(TestSaves.Empty(gameCode: 0));

        save.AddItem(new Gen3BagItem(13, Gen3Pocket.Items, 95)).Should().Be(Gen3BagRefusal.None);
        save.AddItem(new Gen3BagItem(13, Gen3Pocket.Items, 5)).Should().Be(Gen3BagRefusal.StackFull);

        // Ruby/Sapphire's Poké Ball pocket has 16 slots.
        for (int id = 1; id <= 16; id++)
        {
            save.AddItem(new Gen3BagItem(id, Gen3Pocket.Balls, 1)).Should().Be(Gen3BagRefusal.None);
        }

        save.AddItem(new Gen3BagItem(17, Gen3Pocket.Balls, 1)).Should().Be(Gen3BagRefusal.PocketFull);
        Gen3Save.Read(save.ToArray()).ReadPocket(Gen3Pocket.Items).Should().Equal((13, 95));
    }
}
