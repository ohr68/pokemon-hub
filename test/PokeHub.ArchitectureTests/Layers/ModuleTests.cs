using NetArchTest.Rules;
using PokeHub.ArchitectureTests.Abstractions;

namespace PokeHub.ArchitectureTests.Layers;

public class ModuleTests : BaseTest
{
    public static TheoryData<string> AllModules => [.. Modules];

    /// <summary>
    /// A module may only talk to another module through that module's integration events or
    /// its public API (in-process calls, like Trades asking the Pokédex to swap Pokémon).
    /// </summary>
    [Theory]
    [MemberData(nameof(AllModules))]
    public void Module_ShouldNotHaveDependencyOn_AnyOtherModule(string module)
    {
        string[] otherModules = Modules
            .Where(other => other != module)
            .Select(ModuleNamespace)
            .ToArray();

        string[] allowedContracts = Modules
            .Where(other => other != module)
            .SelectMany(other => new[] { IntegrationEventsNamespace(other), PublicApiNamespace(other) })
            .ToArray();

        Types.InAssemblies(ModuleAssemblies(module))
            .That()
            .DoNotHaveDependencyOnAny(allowedContracts)
            .Should()
            .NotHaveDependencyOnAny(otherModules)
            .GetResult()
            .ShouldBeSuccessful();
    }
}
