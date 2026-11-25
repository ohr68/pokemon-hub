using FluentValidation;
using PokeHub.Common.Application.Messaging;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Modules.Games.Application.Abstractions.Identity;
using PokeHub.Modules.Games.Domain.Games;

namespace PokeHub.Modules.Games.Application.DeviceSignIn;

/// <summary>Starts a device's sign-in by code as a registered, active game.</summary>
public sealed record StartDeviceSignInCommand(string ClientId, string CodeChallenge) : ICommand<DeviceSignInCode>;

/// <summary>The game's tokens, once the player approved the code.</summary>
public sealed record GetDeviceTokensCommand(string ClientId, string DeviceCode, string CodeVerifier) : ICommand<DeviceSignInTokens>;

/// <summary>New tokens for a device already signed in.</summary>
public sealed record RefreshDeviceTokensCommand(string ClientId, string RefreshToken) : ICommand<DeviceSignInTokens>;

internal sealed class StartDeviceSignInCommandValidator : AbstractValidator<StartDeviceSignInCommand>
{
    public StartDeviceSignInCommandValidator()
    {
        RuleFor(c => c.ClientId).NotEmpty().MaximumLength(100);
        // A base64url SHA-256 is 43 characters.
        RuleFor(c => c.CodeChallenge).NotEmpty().Length(43, 128);
    }
}

internal sealed class GetDeviceTokensCommandValidator : AbstractValidator<GetDeviceTokensCommand>
{
    public GetDeviceTokensCommandValidator()
    {
        RuleFor(c => c.ClientId).NotEmpty().MaximumLength(100);
        RuleFor(c => c.DeviceCode).NotEmpty().MaximumLength(500);
        RuleFor(c => c.CodeVerifier).NotEmpty().Length(43, 128);
    }
}

internal sealed class RefreshDeviceTokensCommandValidator : AbstractValidator<RefreshDeviceTokensCommand>
{
    public RefreshDeviceTokensCommandValidator()
    {
        RuleFor(c => c.ClientId).NotEmpty().MaximumLength(100);
        RuleFor(c => c.RefreshToken).NotEmpty().MaximumLength(4000);
    }
}

internal sealed class StartDeviceSignInCommandHandler(IGameRepository gameRepository, IDeviceSignInService deviceSignIn)
    : ICommandHandler<StartDeviceSignInCommand, DeviceSignInCode>
{
    public async Task<Result<DeviceSignInCode>> Handle(StartDeviceSignInCommand request, CancellationToken cancellationToken)
    {
        // Only a registered game that's on: the client id is the device's claim to be one.
        Game? game = await gameRepository.GetByClientIdAsync(request.ClientId, cancellationToken);

        if (game is null || !game.IsActive)
        {
            return Result.Failure<DeviceSignInCode>(GameErrors.NotAGame);
        }

        return await deviceSignIn.StartAsync(game.ClientId, request.CodeChallenge, cancellationToken);
    }
}

internal sealed class GetDeviceTokensCommandHandler(IDeviceSignInService deviceSignIn)
    : ICommandHandler<GetDeviceTokensCommand, DeviceSignInTokens>
{
    public Task<Result<DeviceSignInTokens>> Handle(GetDeviceTokensCommand request, CancellationToken cancellationToken)
    {
        return deviceSignIn.GetTokensAsync(request.ClientId, request.DeviceCode, request.CodeVerifier, cancellationToken);
    }
}

internal sealed class RefreshDeviceTokensCommandHandler(IDeviceSignInService deviceSignIn)
    : ICommandHandler<RefreshDeviceTokensCommand, DeviceSignInTokens>
{
    public Task<Result<DeviceSignInTokens>> Handle(RefreshDeviceTokensCommand request, CancellationToken cancellationToken)
    {
        return deviceSignIn.RefreshAsync(request.ClientId, request.RefreshToken, cancellationToken);
    }
}
