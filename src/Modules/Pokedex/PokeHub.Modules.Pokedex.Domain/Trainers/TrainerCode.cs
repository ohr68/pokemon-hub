using System.Security.Cryptography;

namespace PokeHub.Modules.Pokedex.Domain.Trainers;

/// <summary>
/// The public code trainers share to find each other, like a friend code: ABCD-EFGH.
/// Leaves out look-alike characters (0/O, 1/I) so codes are easy to read aloud and type.
/// </summary>
public static class TrainerCode
{
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public const int Length = 9;

    public static string New()
    {
        // 32^8 possible codes; the unique index rejects the rare duplicate.
        Span<char> code = stackalloc char[Length];

        for (int i = 0; i < Length; i++)
        {
            code[i] = i == 4 ? '-' : Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];
        }

        return new string(code);
    }

    /// <summary>
    /// Accepts codes typed in lowercase or without the dash.
    /// </summary>
    public static string Normalize(string code)
    {
        string compact = code.Replace("-", string.Empty, StringComparison.Ordinal).Trim().ToUpperInvariant();

        return compact.Length == Length - 1 ? $"{compact[..4]}-{compact[4..]}" : compact;
    }
}
