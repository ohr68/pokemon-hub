using PokeHub.Common.Domain.Abstractions;

namespace PokeHub.Modules.Games.Application.DeviceSignIn;

/// <summary>Why a device's sign-in by code isn't done, by the codes the device acts on.</summary>
public static class DeviceSignInErrors
{
    public static readonly Error Pending = Error.Problem(
        "DeviceSignIn.Pending",
        "The player hasn't approved the code yet. Ask again after the interval.");

    public static readonly Error SlowDown = Error.Problem(
        "DeviceSignIn.SlowDown",
        "Asked too often. Wait longer between asks.");

    public static readonly Error Expired = Error.Problem(
        "DeviceSignIn.Expired",
        "The code expired before it was approved. Start again.");

    public static readonly Error Denied = Error.Problem(
        "DeviceSignIn.Denied",
        "The player turned the sign-in down.");

    public static readonly Error SignedOut = Error.Problem(
        "DeviceSignIn.SignedOut",
        "The device's sign-in ended. Sign in by code again.");

    public static readonly Error Unavailable = Error.Problem(
        "DeviceSignIn.Unavailable",
        "The sign-in service didn't answer. Try again later.");
}
