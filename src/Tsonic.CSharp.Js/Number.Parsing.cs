using System;
using System.Globalization;
using System.Numerics;

namespace Tsonic.CSharp.Js;

public static partial class Number
{
    private static bool IsWhitespace(char value) =>
        value == '\uFEFF' || value != '\u0085' && char.IsWhiteSpace(value);

    internal static ReadOnlySpan<char> TrimWhitespace(ReadOnlySpan<char> value)
    {
        value = TrimWhitespaceStart(value);
        while (!value.IsEmpty && IsWhitespace(value[^1])) value = value[..^1];
        return value;
    }

    private static ReadOnlySpan<char> TrimWhitespaceStart(ReadOnlySpan<char> value)
    {
        while (!value.IsEmpty && IsWhitespace(value[0])) value = value[1..];
        return value;
    }

    private static int Digit(char value) => value switch
    {
        >= '0' and <= '9' => value - '0',
        >= 'a' and <= 'z' => value - 'a' + 10,
        >= 'A' and <= 'Z' => value - 'A' + 10,
        _ => -1,
    };

    private static double ParseUnsignedInteger(ReadOnlySpan<char> digits, int radix)
    {
        if (radix == 10)
            return double.Parse(digits, NumberStyles.None, CultureInfo.InvariantCulture);
        if (!BitOperations.IsPow2(radix))
        {
            var native = UInt128.Zero;
            BigInteger? wide = null;
            foreach (var character in digits)
            {
                var digit = (uint)Digit(character);
                if (wide.HasValue)
                {
                    wide = wide.Value * radix + digit;
                    if (wide.Value.GetBitLength() > 1024) return double.PositiveInfinity;
                }
                else if (native <= (UInt128.MaxValue - digit) / (uint)radix) native = native * (uint)radix + digit;
                else wide = BigInteger.CreateChecked(native) * radix + digit;
            }
            return wide.HasValue ? (double)wide.Value : (double)native;
        }
        var width = BitOperations.TrailingZeroCount(radix);
        var significant = 0UL;
        var bits = 0;
        var sticky = false;
        foreach (var character in digits)
        {
            var digit = Digit(character);
            for (var index = width - 1; index >= 0; index--)
            {
                var bit = (ulong)((digit >> index) & 1);
                if (bits == 0 && bit == 0) continue;
                if (bits < 54) significant = (significant << 1) | bit;
                else sticky |= bit != 0;
                if (++bits > 1024) return double.PositiveInfinity;
            }
        }
        if (bits <= 53) return significant;
        var rounded = (significant >> 1) + ((significant & 1) != 0 && (sticky || (significant & 2) != 0) ? 1UL : 0UL);
        return double.ScaleB(rounded, bits - 53);
    }

    internal static double ParseNumericString(string source)
    {
        var text = TrimWhitespace(source.AsSpan());
        if (text.IsEmpty) return 0;
        if (text is "Infinity" or "+Infinity") return double.PositiveInfinity;
        if (text is "-Infinity") return double.NegativeInfinity;
        var radix = text.Length > 1 && text[0] == '0'
            ? text[1] switch { 'x' or 'X' => 16, 'o' or 'O' => 8, 'b' or 'B' => 2, _ => 10 }
            : 10;
        if (radix != 10)
        {
            var digits = text[2..];
            if (digits.IsEmpty) return double.NaN;
            foreach (var character in digits)
            {
                var digit = Digit(character);
                if (digit < 0 || digit >= radix) return double.NaN;
            }
            return ParseUnsignedInteger(digits, radix);
        }
        foreach (var character in text)
            if (character is not (>= '0' and <= '9' or '.' or '+' or '-' or 'e' or 'E')) return double.NaN;
        return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var result) ? result : double.NaN;
    }
}
