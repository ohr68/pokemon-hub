using FluentValidation;
using PokeHub.Common.Application.Clock;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Economy.Application.Abstractions.Data;
using PokeHub.Modules.Economy.Domain.Shop;
using PokeHub.Modules.Pokedex.PublicApi;

namespace PokeHub.Modules.Economy.Application.Shop;

/// <param name="Stock">How many can be sold; empty for no limit.</param>
public sealed record CreateOfferCommand(int SpeciesId, int Level, bool IsShiny, int Price, int? Stock) : ICommand<Guid>;

public sealed record UpdateOfferCommand(Guid OfferId, int Price, int? Stock, bool IsActive) : ICommand;

internal sealed class CreateOfferCommandValidator : AbstractValidator<CreateOfferCommand>
{
    public CreateOfferCommandValidator()
    {
        RuleFor(c => c.SpeciesId).GreaterThan(0);
        RuleFor(c => c.Level).InclusiveBetween(1, 100);
        RuleFor(c => c.Price).InclusiveBetween(1, 1_000_000);
        RuleFor(c => c.Stock).GreaterThanOrEqualTo(0);
    }
}

internal sealed class UpdateOfferCommandValidator : AbstractValidator<UpdateOfferCommand>
{
    public UpdateOfferCommandValidator()
    {
        RuleFor(c => c.Price).InclusiveBetween(1, 1_000_000);
        RuleFor(c => c.Stock).GreaterThanOrEqualTo(0);
    }
}

internal sealed class CreateOfferCommandHandler(
    IShopRepository shopRepository,
    IPokedexApi pokedexApi,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateOfferCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateOfferCommand request, CancellationToken cancellationToken)
    {
        SpeciesInfo? species = await pokedexApi.GetSpeciesAsync(request.SpeciesId, cancellationToken);

        if (species is null)
        {
            return Result.Failure<Guid>(ShopErrors.SpeciesNotFound(request.SpeciesId));
        }

        var offer = ShopOffer.Create(
            species.Id,
            species.Name,
            species.IsRare,
            request.Level,
            request.IsShiny,
            request.Price,
            request.Stock,
            dateTimeProvider.UtcNow);

        shopRepository.Insert(offer);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return offer.Id;
    }
}

internal sealed class UpdateOfferCommandHandler(IShopRepository shopRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateOfferCommand>
{
    public async Task<Result> Handle(UpdateOfferCommand request, CancellationToken cancellationToken)
    {
        ShopOffer? offer = await shopRepository.GetOfferAsync(request.OfferId, cancellationToken);

        if (offer is null)
        {
            return Result.Failure(ShopErrors.OfferNotFound(request.OfferId));
        }

        offer.Update(request.Price, request.Stock, request.IsActive);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
