using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Pokedex.Domain.Saves;

public static class SaveErrors
{
    public static Error NotFound(Guid saveId)
    {
        return Error.NotFound("Saves.NotFound", $"The save with the identifier {saveId} was not found.");
    }
}
