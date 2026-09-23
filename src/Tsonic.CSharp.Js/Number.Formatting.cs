using System;
using System.Globalization;
using System.Numerics;

namespace Tsonic.CSharp.Js;

public static partial class Number
{
    private static bool IsFloating<T>() => typeof(T) == typeof(double) ||
        typeof(T) == typeof(float) || typeof(T) == typeof(Half);

    private static string FormatDecimal<T>(T value) where T : INumberBase<T>
    {
        if (!IsFloating<T>()) return value.ToString(null, CultureInfo.InvariantCulture);
        if (T.IsNaN(value)) return "NaN";
        if (T.IsPositiveInfinity(value)) return "Infinity";
        if (T.IsNegativeInfinity(value)) return "-Infinity";
        if (T.IsZero(value)) return "0";
        Span<char> source = stackalloc char[64];
        if (!value.TryFormat(source, out var length, "R", CultureInfo.InvariantCulture))
            throw new InvalidOperationException("Native floating formatting exceeded its bounded buffer.");
        Span<char> digits = stackalloc char[101];
        var count = ReadDigits(source[..length], digits, out var exponent, out var negative);
        return RenderDigits(digits[..count], exponent, negative, exponent < -6 || exponent >= 21);
    }

    private static string FormatSignificant<T>(T value, int? precision, bool exponential) where T : INumberBase<T>
    {
        Span<char> digits = stackalloc char[101];
        Span<char> source = stackalloc char[128];
        var nativeFormat = IsFloating<T>() ? "R" : null;
        if (!value.TryFormat(source, out var length, nativeFormat, CultureInfo.InvariantCulture))
            throw new InvalidOperationException("Native numeric formatting exceeded its bounded buffer.");
        var count = ReadDigits(source[..length], digits, out var exponent, out var negative);
        if (precision.HasValue && IsFloating<T>())
        {
            Span<char> formatBuffer = stackalloc char[4];
            var format = PrecisionFormat('E', precision.Value - 1, formatBuffer);
            var increment = DecimalHalfRoundsDown(value, precision.Value - 1 - exponent);
            if (!value.TryFormat(source, out length, format, CultureInfo.InvariantCulture))
                throw new InvalidOperationException("Significant formatting exceeded its bounded buffer.");
            count = ReadDigits(source[..length], digits, out exponent, out negative);
            if (increment) digits[count - 1]++;
        }
        if (precision.HasValue)
        {
            var requested = precision.Value;
            if (count > requested && digits[requested] >= '5')
            {
                var cursor = requested;
                while (cursor > 0 && digits[cursor - 1] == '9') digits[--cursor] = '0';
                if (cursor == 0) { digits[0] = '1'; exponent++; }
                else digits[cursor - 1]++;
            }
            if (count < requested) digits[count..requested].Fill('0');
            count = requested;
        }
        else
        {
            while (count > 1 && digits[count - 1] == '0') count--;
        }
        return RenderDigits(digits[..count], exponent, negative,
            exponential || exponent < -6 || exponent >= count);
    }

    private static ReadOnlySpan<char> PrecisionFormat(char kind, int precision, Span<char> buffer)
    {
        buffer[0] = kind;
        if (!precision.TryFormat(buffer[1..], out var length, default, CultureInfo.InvariantCulture))
            throw new InvalidOperationException("Numeric precision exceeded its bounded format buffer.");
        return buffer[..(length + 1)];
    }

    private static bool DecimalHalfRoundsDown<T>(T value, int decimals) where T : INumberBase<T>
    {
        if (!IsFloating<T>() || !T.IsFinite(value) || T.IsZero(value)) return false;
        var magnitude = System.Math.Abs(double.CreateChecked(value));
        var bits = BitConverter.DoubleToUInt64Bits(magnitude);
        var rawExponent = (int)((bits >> 52) & 0x7ff);
        var significand = bits & ((1UL << 52) - 1);
        var exponent = rawExponent == 0 ? -1074 : rawExponent - 1075;
        if (rawExponent != 0) significand |= 1UL << 52;
        for (var index = decimals; index < 0; index++)
        {
            if (significand % 5 != 0) return false;
            significand /= 5;
        }
        var denominatorBits = -(exponent + decimals);
        if (denominatorBits < 1 || denominatorBits > 64 ||
            BitOperations.TrailingZeroCount(significand) != denominatorBits - 1) return false;
        return ((significand >> (denominatorBits - 1)) & 3) == 1;
    }

    private static int ReadDigits(ReadOnlySpan<char> source, Span<char> digits, out int exponent, out bool negative)
    {
        negative = source[0] == '-';
        if (negative) source = source[1..];
        var exponentAt = source.IndexOfAny('E', 'e');
        var suffix = exponentAt < 0 ? 0 : int.Parse(source[(exponentAt + 1)..], CultureInfo.InvariantCulture);
        if (exponentAt >= 0) source = source[..exponentAt];
        var point = source.IndexOf('.');
        if (point < 0) point = source.Length;
        var count = 0;
        var leading = 0;
        foreach (var character in source)
        {
            if (character == '.') continue;
            if (count == 0 && character == '0') { leading++; continue; }
            digits[count++] = character;
        }
        exponent = point - leading - 1 + suffix;
        if (count == 0) { digits[0] = '0'; exponent = 0; negative = false; return 1; }
        return count;
    }

    private static string RenderDigits(ReadOnlySpan<char> digits, int exponent, bool negative, bool exponential)
    {
        Span<char> output = stackalloc char[432];
        var written = 0;
        if (negative) output[written++] = '-';
        if (exponential)
        {
            output[written++] = digits[0];
            if (digits.Length > 1)
            {
                output[written++] = '.';
                digits[1..].CopyTo(output[written..]);
                written += digits.Length - 1;
            }
            output[written++] = 'e';
            if (exponent >= 0) output[written++] = '+';
            exponent.TryFormat(output[written..], out var exponentLength, default, CultureInfo.InvariantCulture);
            written += exponentLength;
        }
        else
        {
            var point = exponent + 1;
            if (point <= 0)
            {
                output[written++] = '0';
                output[written++] = '.';
                output.Slice(written, -point).Fill('0');
                written -= point;
            }
            for (var index = 0; index < digits.Length; index++)
            {
                if (index > 0 && index == point) output[written++] = '.';
                output[written++] = digits[index];
            }
            if (point > digits.Length)
            {
                output.Slice(written, point - digits.Length).Fill('0');
                written += point - digits.Length;
            }
        }
        return new string(output[..written]);
    }
}
