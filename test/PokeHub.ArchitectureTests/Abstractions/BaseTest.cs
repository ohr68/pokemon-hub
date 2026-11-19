using System.Reflection;

namespace PokeHub.ArchitectureTests.Abstractions;

#pragma warning disable CA1515
public abstract class BaseTest
#pragma warning restore CA1515
{
    protected const string ModulesNamespace = "PokeHub.Modules";

    // Add new modules here; every module is checked against all the others.
    protected static readonly string[] Modules = ["Users", "Pokedex", "Trades"];

    private static readonly string[] ModuleLayers = ["Domain", "Application", "Infrastructure", "Presentation"];

    protected static string ModuleNamespace(string module) => $"{ModulesNamespace}.{module}";

    protected static string IntegrationEventsNamespace(string module) =>
        $"{ModuleNamespace(module)}.IntegrationEvents";

    // Loaded by name because a new module's projects can still be empty, leaving no type to use with typeof.
    protected static Assembly[] ModuleAssemblies(string module) =>
        ModuleLayers
            .Select(layer => Assembly.Load($"{ModuleNamespace(module)}.{layer}"))
            .ToArray();
}
