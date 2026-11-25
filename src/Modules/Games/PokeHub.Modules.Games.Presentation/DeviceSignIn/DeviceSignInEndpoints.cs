using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Presentation.Endpoints;
using PokeHub.Modules.Games.Application.Abstractions.Identity;
using PokeHub.Modules.Games.Application.DeviceSignIn;
using PokeHub.Modules.Games.Presentation.Games;

namespace PokeHub.Modules.Games.Presentation.DeviceSignIn;

// Sign-in by code for devices without a browser, like the 3DS sync app. Open to anyone, like
// the identity provider's own endpoints they stand in for: what proves the player is the code
// they approve while signed in, and then the tokens.

internal sealed class StartDeviceSignInRequest
{
    public string ClientId { get; init; } = string.Empty;

    public string CodeChallenge { get; init; } = string.Empty;
}

internal sealed class GetDeviceTokensRequest
{
    public string ClientId { get; init; } = string.Empty;

    public string DeviceCode { get; init; } = string.Empty;

    public string CodeVerifier { get; init; } = string.Empty;
}

internal sealed class RefreshDeviceTokensRequest
{
    public string ClientId { get; init; } = string.Empty;

    public string RefreshToken { get; init; } = string.Empty;
}

internal sealed class StartDeviceSignInEndpoint(ISender sender) : EndpointBase<StartDeviceSignInRequest>
{
    public override void Configure()
    {
        Post("device/code");
        AllowAnonymous();
        Group<GamesEndpointGroup>();

        Description(builder => builder.Produces<DeviceSignInCode>(StatusCodes.Status200OK, "application/json"));
        Summary(summary =>
        {
            summary.Summary = "Starts a device's sign-in by code";
            summary.Description = "For a device without a browser, as a registered, active game (its client id) with a PKCE challenge (S256). Returns the code the player approves on another device, the approval page, and the device code to ask for tokens with. Open to everyone.";
        });
    }

    public override async Task HandleAsync(StartDeviceSignInRequest req, CancellationToken ct)
    {
        Result<DeviceSignInCode> result = await sender.Send(new StartDeviceSignInCommand(req.ClientId, req.CodeChallenge), ct);

        await SendResultAsync(result, code => Results.Ok(code));
    }
}

internal sealed class GetDeviceTokensEndpoint(ISender sender) : EndpointBase<GetDeviceTokensRequest>
{
    public override void Configure()
    {
        Post("device/token");
        AllowAnonymous();
        Group<GamesEndpointGroup>();

        Description(builder => builder.Produces<DeviceSignInTokens>(StatusCodes.Status200OK, "application/json"));
        Summary(summary =>
        {
            summary.Summary = "Gets a device's tokens once its code is approved";
            summary.Description = "Asked every few seconds until the player approves the code. Until then it answers DeviceSignIn.Pending (or SlowDown, Expired, Denied). Open to everyone.";
        });
    }

    public override async Task HandleAsync(GetDeviceTokensRequest req, CancellationToken ct)
    {
        Result<DeviceSignInTokens> result = await sender.Send(new GetDeviceTokensCommand(req.ClientId, req.DeviceCode, req.CodeVerifier), ct);

        await SendResultAsync(result, tokens => Results.Ok(tokens));
    }
}

internal sealed class RefreshDeviceTokensEndpoint(ISender sender) : EndpointBase<RefreshDeviceTokensRequest>
{
    public override void Configure()
    {
        Post("device/refresh");
        AllowAnonymous();
        Group<GamesEndpointGroup>();

        Description(builder => builder.Produces<DeviceSignInTokens>(StatusCodes.Status200OK, "application/json"));
        Summary(summary =>
        {
            summary.Summary = "Refreshes a device's tokens";
            summary.Description = "With the refresh token the device kept. DeviceSignIn.SignedOut means it must sign in by code again. Open to everyone.";
        });
    }

    public override async Task HandleAsync(RefreshDeviceTokensRequest req, CancellationToken ct)
    {
        Result<DeviceSignInTokens> result = await sender.Send(new RefreshDeviceTokensCommand(req.ClientId, req.RefreshToken), ct);

        await SendResultAsync(result, tokens => Results.Ok(tokens));
    }
}
