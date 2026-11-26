using FluentAssertions;
using PokeHub.Modules.Games.Domain.SaveEdits;

namespace PokeHub.Modules.Games.UnitTests.SaveEdits;

public class SaveEditTests
{
    [Fact]
    public void MissingFrom_ShouldList_TheDeliveredPokemonNotInTheSave()
    {
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        var edit = SaveEdit.Create(Guid.NewGuid(), Guid.NewGuid(), [(first, "gen3-a"), (second, "gen3-b")], [], [], DateTime.UtcNow);

        edit.MissingFrom(new HashSet<string> { "gen3-a" }).Should().Equal(second);
        edit.MissingFrom(new HashSet<string> { "gen3-a", "gen3-b" }).Should().BeEmpty();
    }

    [Fact]
    public void StillIn_ShouldList_TheWithdrawnPokemonStillInTheSave()
    {
        var edit = SaveEdit.Create(Guid.NewGuid(), Guid.NewGuid(), [], ["gen3-c", "gen3-d"], [], DateTime.UtcNow);

        edit.StillIn(new HashSet<string> { "gen3-c", "gen3-x" }).Should().Equal("gen3-c");
        edit.StillIn(new HashSet<string>()).Should().BeEmpty();
    }
}
