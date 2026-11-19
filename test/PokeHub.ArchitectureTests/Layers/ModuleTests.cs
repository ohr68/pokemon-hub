using NetArchTest.Rules;
using PokeHub.ArchitectureTests.Abstractions;

namespace PokeHub.ArchitectureTests.Layers;

public class ModuleTests : BaseTest
{
    public static TheoryData<string> AllModules => [.. Modules];

    /// <summary>
    /// A module may only talk to another module through that module's integration events.
    /// </summary>
    [Theory]
    [MemberData(nameof(AllModules))]
    public void Module_ShouldNotHaveDependencyOn_AnyOtherModule(string module)
    {
        string[] otherModules = Modules
            .Where(other => other != module)
            .Select(ModuleNamespace)
            .ToArray();

        string[] otherIntegrationEvents = Modules
            .Where(other => other != module)
            .Select(IntegrationEventsNamespace)
            .ToArray();

        Types.InAssemblies(ModuleAssemblies(module))
            .That()
            .DoNotHaveDependencyOnAny(otherIntegrationEvents)
            .Should()
            .NotHaveDependencyOnAny(otherModules)
            .GetResult()
            .ShouldBeSuccessful();
    }
}
