using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Games.Domain.Games;
using PokeHub.Modules.Games.Domain.SaveEdits;
using PokeHub.Modules.Games.Domain.SaveFormats.Gen3;

namespace PokeHub.Modules.Games.Application.SaveEdits;

internal static class SaveEditReader
{
    /// <summary>The game's save, read as the Gen 3 game it is; refused for a game PokeHub doesn't edit.</summary>
    internal static Result<(Gen3Save Save, Gen3Game Game)> Read(Game game, byte[] content)
    {
        var gen3Game = Gen3Game.ForSlug(game.Slug);
        if (gen3Game is null)
        {
            return Result.Failure<(Gen3Save, Gen3Game)>(SaveEditErrors.NotEditable);
        }

        Gen3Save save;
        try
        {
            save = Gen3Save.Read(content, gen3Game.Family);
        }
        catch (Gen3SaveException exception)
        {
            return Result.Failure<(Gen3Save, Gen3Game)>(SaveEditErrors.Unreadable(exception.Message));
        }

        return save.Layout.Family == gen3Game.Family
            ? (save, gen3Game)
            : Result.Failure<(Gen3Save, Gen3Game)>(SaveEditErrors.Unreadable($"This save isn't from a {gen3Game.Family} game."));
    }
}
