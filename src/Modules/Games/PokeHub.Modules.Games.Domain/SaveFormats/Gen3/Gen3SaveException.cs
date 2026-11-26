namespace PokeHub.Modules.Games.Domain.SaveFormats.Gen3;

/// <summary>A save PokeHub can't read or change, with the reason for the player.</summary>
public sealed class Gen3SaveException : Exception
{
    public Gen3SaveException()
    {
    }

    public Gen3SaveException(string message)
        : base(message)
    {
    }

    public Gen3SaveException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
