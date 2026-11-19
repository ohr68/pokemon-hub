using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Common.Application.Exceptions;

public sealed class PokeHubException : Exception
{
    public PokeHubException(string requestName, Error? error = null, Exception? innerException = null)
        : base("Application exception", innerException)
    {
        RequestName = requestName;
        Error = error;
    }

    public string RequestName { get; }
    public Error? Error { get; }
}
