using System.Reflection;

namespace PokeHub.Modules.Games.Application;

public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
