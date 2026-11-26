using FluentAssertions;
using PokeHub.Modules.Games.Domain.SaveFormats.Gen3;

namespace PokeHub.Modules.Games.UnitTests.SaveFormats.Gen3;

public class Gen3NewPokemonTests
{
    private static readonly Dictionary<int, int> MovePp = new() { [10] = 35, [45] = 40, [52] = 25, [99] = 20, [108] = 20, [400] = 10 };

    private static readonly Gen3Species Charmander = new(
        4,
        "charmander",
        [0, 9, 57, 96, 135, 179, 236, 314, 419, 560],
        [(1, 10), (1, 45), (7, 52), (13, 108), (19, 99), (5, 400)]);

    [Fact]
    public void MovesAtLevel_ShouldKeepTheLastFourLearned_UpToTheLevel()
    {
        Gen3NewPokemon.MovesAtLevel(Charmander, 7, MovePp)
            .Should().Equal(new Pk3Move(10, 35), new Pk3Move(45, 40), new Pk3Move(52, 25));
        Gen3NewPokemon.MovesAtLevel(Charmander, 20, MovePp)
            .Select(m => m.Id).Should().Equal(45, 52, 108, 99);
    }

    [Fact]
    public void MovesAtLevel_ShouldSkipMovesAfterGen3_AndFallBackToTackle()
    {
        Gen3NewPokemon.MovesAtLevel(Charmander, 5, MovePp).Select(m => m.Id).Should().NotContain(400);
        Gen3NewPokemon.MovesAtLevel(Charmander with { LevelUpMoves = [] }, 5, MovePp).Should().Equal(new Pk3Move(33, 35));
    }

    [Theory]
    [InlineData("nidoran-f", "NIDORAN♀")]
    [InlineData("mr-mime", "MR. MIME")]
    [InlineData("charmander", "CHARMANDER")]
    [InlineData("tapu-koko", "TAPU KOKO")]
    public void DisplayName_ShouldNameItLikeTheGame(string species, string expected)
    {
        Gen3NewPokemon.DisplayName(species).Should().Be(expected);
    }

    [Fact]
    public void ForeignTrainerId_ShouldBeStable_AndNeverThePlayers()
    {
        const string trainer = "01a1f978-4255-7b21-829f-45b8105990e0";
        uint id = Gen3NewPokemon.ForeignTrainerId(trainer, 0);

        Gen3NewPokemon.ForeignTrainerId(trainer, 0).Should().Be(id);
        Gen3NewPokemon.ForeignTrainerId(trainer, id).Should().Be(id ^ 1);
    }

    [Fact]
    public void Create_ShouldMakeThePlayersOwnPokemon_MetInThisGame()
    {
        var save = Gen3Save.Read(TestSaves.Empty());
        Gen3Game game = Gen3Game.ForSlug("twin-embers")!;

        byte[] bytes = Gen3NewPokemon.Create(
            new HubPokemon(null, 5, IsShiny: true, IsOriginalTrainer: true, "someone", "ASH"),
            Charmander,
            save,
            game,
            MovePp);
        save.SetSlot(0, bytes);
        Gen3Pokemon pokemon = Gen3Save.Read(save.ToArray()).Pokemon().Single();

        pokemon.Should().BeEquivalentTo(new
        {
            Gen3Species = 4,
            Experience = 135u,
            Nickname = "CHARMANDER",
            IsShiny = true,
            OriginalTrainerId = TestSaves.TrainerId,
            OriginGame = 4,
        });
    }

    [Fact]
    public void Create_ShouldGiveATradedPokemon_ItsOwnTrainer()
    {
        var save = Gen3Save.Read(TestSaves.Empty());

        byte[] bytes = Gen3NewPokemon.Create(
            new HubPokemon("Blaze", 3, IsShiny: false, IsOriginalTrainer: false, "another-trainer", "Misty"),
            Charmander,
            save,
            Gen3Game.ForSlug("pokemon-emerald")!,
            MovePp);
        save.SetSlot(0, bytes);
        Gen3Pokemon pokemon = Gen3Save.Read(save.ToArray()).Pokemon().Single();

        pokemon.Nickname.Should().Be("Blaze");
        pokemon.OriginalTrainerId.Should().Be(Gen3NewPokemon.ForeignTrainerId("another-trainer", TestSaves.TrainerId));
        pokemon.OriginGame.Should().Be(3);
        Gen3Text.Decode(bytes.AsSpan(0x14, 7)).Should().Be("MISTY");
    }

    [Theory]
    [InlineData("firered", Gen3Family.FireRedLeafGreen)]
    [InlineData("pokemon-ruby", Gen3Family.RubySapphire)]
    [InlineData("emerald-3ds", Gen3Family.Emerald)]
    public void ForSlug_ShouldKnowTheGen3Games(string slug, Gen3Family family)
    {
        Gen3Game.ForSlug(slug)!.Family.Should().Be(family);
    }

    [Fact]
    public void ForSlug_ShouldNotKnow_OtherGames()
    {
        Gen3Game.ForSlug("pokemon-x").Should().BeNull();
    }
}
