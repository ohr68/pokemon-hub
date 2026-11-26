using System.Text;

namespace PokeHub.Modules.Games.Domain.SaveFormats.Gen3;

/// <summary>
/// The Gen 3 character set (English games): letters, digits and common punctuation. Strings
/// end with 0xFF; anything outside the set reads as "?". Mirrors pokemon-web's
/// <c>src/lib/gen3/text.ts</c>.
/// </summary>
public static class Gen3Text
{
    private const byte End = 0xFF;
    private const byte QuestionMark = 0xAC;

    private static readonly Dictionary<byte, char> Punctuation = new()
    {
        [0x00] = ' ',
        [0xAB] = '!',
        [0xAC] = '?',
        [0xAD] = '.',
        [0xAE] = '-',
        [0xB0] = '…',
        [0xB1] = '“',
        [0xB2] = '”',
        [0xB3] = '‘',
        [0xB4] = '’',
        [0xB5] = '♂',
        [0xB6] = '♀',
        [0xB8] = ',',
        [0xBA] = '/',
    };

    private static readonly Dictionary<char, byte> PunctuationBytes = BuildPunctuationBytes();

    public static string Decode(ReadOnlySpan<byte> bytes)
    {
        var text = new StringBuilder();

        foreach (byte b in bytes)
        {
            if (b == End)
            {
                break;
            }

            text.Append(DecodeChar(b));
        }

        return text.ToString().Trim();
    }

    /// <summary>
    /// Encodes text into a fixed-size field, ended (and padded) with 0xFF. Text longer than the
    /// field is cut; characters the game can't show become "?".
    /// </summary>
    public static byte[] Encode(string text, int length)
    {
        ArgumentNullException.ThrowIfNull(text);

        byte[] bytes = new byte[length];
        Array.Fill(bytes, End);

        for (int i = 0; i < Math.Min(text.Length, length); i++)
        {
            bytes[i] = EncodeChar(text[i]);
        }

        return bytes;
    }

    private static char DecodeChar(byte b) => b switch
    {
        >= 0xBB and <= 0xD4 => (char)('A' + b - 0xBB),
        >= 0xD5 and <= 0xEE => (char)('a' + b - 0xD5),
        >= 0xA1 and <= 0xAA => (char)('0' + b - 0xA1),
        _ => Punctuation.GetValueOrDefault(b, '?'),
    };

    private static byte EncodeChar(char c) => c switch
    {
        >= 'A' and <= 'Z' => (byte)(0xBB + c - 'A'),
        >= 'a' and <= 'z' => (byte)(0xD5 + c - 'a'),
        >= '0' and <= '9' => (byte)(0xA1 + c - '0'),
        _ => PunctuationBytes.GetValueOrDefault(c, QuestionMark),
    };

    private static Dictionary<char, byte> BuildPunctuationBytes()
    {
        var bytes = Punctuation.ToDictionary(pair => pair.Value, pair => pair.Key);

        // Typed equivalents of the game's curly quotes.
        bytes['\''] = 0xB4;
        bytes['"'] = 0xB2;
        return bytes;
    }
}
