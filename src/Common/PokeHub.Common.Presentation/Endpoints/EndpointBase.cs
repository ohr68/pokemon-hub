using FastEndpoints;
using Microsoft.AspNetCore.Http;
using PokeHub.Common.Domain.Abstractions;
using PokeHub.Common.Presentation.ApiResults;

namespace PokeHub.Common.Presentation.Endpoints;

/// <summary>
/// Base for endpoints that bind a request. Handlers send a command or query and pass the
/// <see cref="Result"/> to <c>SendResultAsync</c>, which writes the success response or a
/// problem details response built from the result's error.
/// </summary>
public abstract class EndpointBase<TRequest> : Endpoint<TRequest>
    where TRequest : notnull
{
    protected Task SendResultAsync<TValue>(Result<TValue> result, Func<TValue, IResult> onSuccess) =>
        Send.ResultAsync(result.Match(onSuccess, ApiResults.ApiResults.Problem));

    protected Task SendResultAsync(Result result, Func<IResult> onSuccess) =>
        Send.ResultAsync(result.Match(onSuccess, ApiResults.ApiResults.Problem));
}

/// <summary>
/// Base for endpoints without a request body or route values, such as reads for the current user.
/// </summary>
public abstract class EndpointWithoutRequestBase : EndpointWithoutRequest
{
    protected Task SendResultAsync<TValue>(Result<TValue> result, Func<TValue, IResult> onSuccess) =>
        Send.ResultAsync(result.Match(onSuccess, ApiResults.ApiResults.Problem));

    protected Task SendResultAsync(Result result, Func<IResult> onSuccess) =>
        Send.ResultAsync(result.Match(onSuccess, ApiResults.ApiResults.Problem));
}
