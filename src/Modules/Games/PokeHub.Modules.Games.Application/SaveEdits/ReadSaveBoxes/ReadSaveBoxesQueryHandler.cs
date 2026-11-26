using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Games.Application.Play;
using PokeHub.Modules.Games.Domain.Games;
using PokeHub.Modules.Games.Domain.SaveFormats.Gen3;
using PokeHub.Modules.Pokedex.PublicApi;

namespace PokeHub.Modules.Games.Application.SaveEdits.ReadSaveBoxes;

internal sealed class ReadSaveBoxesQueryHandler(IGameRepository gameRepository, IPokedexApi pokedexApi)
    : IQueryHandler<ReadSaveBoxesQuery, SaveBoxesResponse>
{
    public async Task<Result<SaveBoxesResponse>> Handle(ReadSaveBoxesQuery request, CancellationToken cancellationToken)
    {
        Result<Game> game = await PlayingGame.ResolveAsync(gameRepository, request.ClientId, cancellationToken);
        if (game.IsFailure)
        {
            return Result.Failure<SaveBoxesResponse>(game.Error);
        }

        Result<(Gen3Save Save, Gen3Game Game)> read = SaveEditReader.Read(game.Value, request.Content);
        if (read.IsFailure)
        {
            return Result.Failure<SaveBoxesResponse>(read.Error);
        }

        Gen3Catalog catalog = await pokedexApi.GetGen3CatalogAsync(cancellationToken);
        var speciesByIndex = catalog.Species.ToDictionary(s => s.Gen3Index);

        return new SaveBoxesResponse([.. read.Value.Save.Pokemon()
            .Where(p => p.Location == Gen3Location.Box && speciesByIndex.ContainsKey(p.Gen3Species))
            .Select(p =>
            {
                Gen3SpeciesInfo species = speciesByIndex[p.Gen3Species];
                (int box, int slot) = Gen3Save.BoxPosition(p.BoxSlot!.Value);
                int level = Gen3NewPokemon.LevelFromExperience(
                    p.Experience,
                    species.GrowthRate is not null && catalog.ExperienceByLevel.TryGetValue(species.GrowthRate, out int[]? table) ? table : []);

                return new BoxPokemonResponse(
                    box,
                    slot,
                    species.SpeciesId,
                    Gen3NewPokemon.RealNickname(p.Nickname, species.Name) ?? Gen3NewPokemon.DisplayName(species.Name),
                    level,
                    p.IsShiny,
                    p.CatchKey);
            })]);
    }
}
