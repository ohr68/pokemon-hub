using FluentAssertions;
using PokeHub.Modules.Games.Domain.SaveFormats.Gen3;

namespace PokeHub.Modules.Games.UnitTests.SaveFormats.Gen3;

public class Pk3Tests
{
    [Fact]
    public void PersonalityFor_ShouldMakeShinyOrNot_AsAsked()
    {
        for (int i = 0; i < 200; i++)
        {
            IsShiny(Pk3.PersonalityFor(TestSaves.TrainerId, isShiny: true)).Should().BeTrue();
            IsShiny(Pk3.PersonalityFor(TestSaves.TrainerId, isShiny: false)).Should().BeFalse();
        }
    }

    [Fact]
    public void Decrypt_ShouldReturnThePlainData_WhenTheChecksumMatches()
    {
        byte[] pokemon = TestSaves.Charmander(0x12345678);

        Pk3.Decrypt(pokemon).Should().NotBeNull();
        pokemon[0x30] ^= 1;
        Pk3.Decrypt(pokemon).Should().BeNull();
    }

    [Fact]
    public void RandomIvs_ShouldGiveSixValues_From0To31()
    {
        Pk3.RandomIvs().Should().HaveCount(6).And.OnlyContain(iv => iv >= 0 && iv <= 31);
    }

    [Theory]
    [InlineData("CHARMANDER")]
    [InlineData("Mr. Mime")]
    [InlineData("NIDORAN♀")]
    public void Text_ShouldRoundTrip_TheGamesCharacters(string text)
    {
        Gen3Text.Decode(Gen3Text.Encode(text, 10)).Should().Be(text);
    }

    [Fact]
    public void Text_ShouldCutLongText_AndShowUnknownCharactersAsQuestionMarks()
    {
        Gen3Text.Decode(Gen3Text.Encode("ABCDEFGHIJKL", 10)).Should().Be("ABCDEFGHIJ");
        Gen3Text.Decode(Gen3Text.Encode("Ñ", 10)).Should().Be("?");
    }

    private static bool IsShiny(uint personality) =>
        ((TestSaves.TrainerId & 0xFFFF) ^ (TestSaves.TrainerId >> 16) ^ (personality & 0xFFFF) ^ (personality >> 16)) < 8;
}
