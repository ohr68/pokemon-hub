using FluentValidation;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Games.Application.Abstractions.Data;
using PokeHub.Modules.Games.Application.Abstractions.Identity;
using PokeHub.Modules.Games.Domain.Games;

namespace PokeHub.Modules.Games.Application.Games.UpdateGame;

/// <param name="RateLimitPerMinute">Requests one player may make to the game API per minute.</param>
public sealed record UpdateGameCommand(Guid GameId, string Name, string? Description, int RateLimitPerMinute) : ICommand;

internal sealed class UpdateGameCommandValidator : AbstractValidator<UpdateGameCommand>
{
    public UpdateGameCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Description).MaximumLength(1000);
        RuleFor(c => c.RateLimitPerMinute).InclusiveBetween(Game.MinRateLimitPerMinute, Game.MaxRateLimitPerMinute);
    }
}

internal sealed class UpdateGameCommandHandler(
    IGameRepository gameRepository,
    IGameOriginCache gameCache,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateGameCommand>
{
    public async Task<Result> Handle(UpdateGameCommand request, CancellationToken cancellationToken)
    {
        Game? game = await gameRepository.GetAsync(request.GameId, cancellationToken);

        if (game is null)
        {
            return Result.Failure(GameErrors.NotFound(request.GameId));
        }

        game.UpdateDetails(request.Name.Trim(), request.Description, request.RateLimitPerMinute);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // The rate limit is read from the cache.
        gameCache.Invalidate();

        return Result.Success();
    }
}
