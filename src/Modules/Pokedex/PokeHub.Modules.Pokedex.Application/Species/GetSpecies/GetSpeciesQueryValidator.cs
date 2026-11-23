using FluentValidation;
using PokeHub.Modules.Pokedex.Domain.Species;

namespace PokeHub.Modules.Pokedex.Application.Species.GetSpecies;

internal sealed class GetSpeciesQueryValidator : AbstractValidator<GetSpeciesQuery>
{
    public GetSpeciesQueryValidator()
    {
        RuleFor(q => q.Page).GreaterThanOrEqualTo(1);

        RuleFor(q => q.PageSize).InclusiveBetween(1, 200);

        RuleFor(q => q.Generation).GreaterThanOrEqualTo(1).When(q => q.Generation is not null);

        RuleFor(q => q.Game)
            .Must(game => GameDexes.Find(game!) is not null)
            .When(q => !string.IsNullOrWhiteSpace(q.Game))
            .WithMessage("Unknown game.");
    }
}
