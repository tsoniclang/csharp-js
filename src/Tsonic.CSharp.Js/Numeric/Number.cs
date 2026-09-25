using System;
using System.Globalization;
using System.Numerics;

namespace Tsonic.CSharp.Js
{
    public static partial class Number
    {
        public const double MAX_VALUE = double.MaxValue;
        public const double MIN_VALUE = double.Epsilon; // Smallest positive value
        public const double MAX_SAFE_INTEGER = 9007199254740991; // 2^53 - 1
        public const double MIN_SAFE_INTEGER = -9007199254740991; // -(2^53 - 1)
        public const double POSITIVE_INFINITY = double.PositiveInfinity;
        public const double NEGATIVE_INFINITY = double.NegativeInfinity;
        public const double NaN = double.NaN;
        public const double EPSILON = 2.220446049250313e-16; // 2^-52


        public static double parseInt(string str, int? radix = null)
        {
            if (string.IsNullOrEmpty(str))
            {
                return double.NaN;
            }

            var span = TrimWhitespaceStart(str.AsSpan());
            if (span.Length == 0)
            {
                return double.NaN;
            }

            var sign = 1.0;
            if (span[0] == '+' || span[0] == '-')
            {
                if (span[0] == '-')
                {
                    sign = -1.0;
                }

                span = span[1..];
                if (span.Length == 0)
                {
                    return double.NaN;
                }
            }

            var actualRadix = radix ?? 0;
            if (actualRadix != 0 && (actualRadix < 2 || actualRadix > 36))
            {
                return double.NaN;
            }

            if (actualRadix == 0)
            {
                actualRadix = 10;
                if (span.Length >= 2 && span[0] == '0' && (span[1] == 'x' || span[1] == 'X'))
                {
                    actualRadix = 16;
                    span = span[2..];
                }
            }
            else if (actualRadix == 16 && span.Length >= 2 && span[0] == '0' && (span[1] == 'x' || span[1] == 'X'))
            {
                span = span[2..];
            }

            var count = 0;
            foreach (var character in span)
            {
                var digit = Digit(character);
                if (digit < 0 || digit >= actualRadix) break;
                count++;
            }
            return count == 0 ? double.NaN : sign * ParseUnsignedInteger(span[..count], actualRadix);
        }

        public static double parseFloat(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return double.NaN;
            }

            var span = TrimWhitespaceStart(str.AsSpan());
            if (span.Length == 0)
            {
                return double.NaN;
            }

            var cursor = 0;
            if (span[cursor] == '+' || span[cursor] == '-')
            {
                cursor++;
            }

            if (span[cursor..].StartsWith("Infinity", StringComparison.Ordinal))
            {
                return cursor > 0 && span[0] == '-'
                    ? double.NegativeInfinity
                    : double.PositiveInfinity;
            }

            var digitStart = cursor;
            while (cursor < span.Length && char.IsAsciiDigit(span[cursor]))
            {
                cursor++;
            }

            if (cursor < span.Length && span[cursor] == '.')
            {
                cursor++;
                while (cursor < span.Length && char.IsAsciiDigit(span[cursor]))
                {
                    cursor++;
                }
            }

            if (cursor == digitStart || (cursor == digitStart + 1 && span[digitStart] == '.'))
            {
                return double.NaN;
            }

            var exponentStart = cursor;
            if (cursor < span.Length && (span[cursor] == 'e' || span[cursor] == 'E'))
            {
                cursor++;
                if (cursor < span.Length && (span[cursor] == '+' || span[cursor] == '-'))
                {
                    cursor++;
                }

                var exponentDigitStart = cursor;
                while (cursor < span.Length && char.IsAsciiDigit(span[cursor]))
                {
                    cursor++;
                }

                if (cursor == exponentDigitStart)
                {
                    cursor = exponentStart;
                }
            }

            return double.TryParse(span[..cursor], NumberStyles.Float, CultureInfo.InvariantCulture, out var result)
                ? result
                : (span[0] == '-' ? double.NegativeInfinity : double.PositiveInfinity);
        }

        public static bool isNaN<T>(T value) where T : System.Numerics.INumberBase<T> =>
            T.IsNaN(value);

        public static bool isNaN<T>(T? value) where T : struct, System.Numerics.INumberBase<T> =>
            value.HasValue && T.IsNaN(value.Value);

        public static bool isFinite<T>(T value) where T : System.Numerics.INumberBase<T> =>
            T.IsFinite(value);

        public static bool isFinite<T>(T? value) where T : struct, System.Numerics.INumberBase<T> =>
            value.HasValue && T.IsFinite(value.Value);

        public static bool isInteger<T>(T value) where T : System.Numerics.INumberBase<T> =>
            T.IsInteger(value);

        public static bool isInteger<T>(T? value) where T : struct, System.Numerics.INumberBase<T> =>
            value.HasValue && T.IsInteger(value.Value);

        public static bool isSafeInteger<T>(T value) where T : INumber<T>
        {
            if (!T.IsInteger(value)) return false;
            if (typeof(T) == typeof(float)) return System.Math.Abs(double.CreateChecked(value)) < 9007199254740992.0;
            if (typeof(T) == typeof(sbyte) || typeof(T) == typeof(byte) ||
                typeof(T) == typeof(short) || typeof(T) == typeof(ushort) ||
                typeof(T) == typeof(int) || typeof(T) == typeof(uint) || typeof(T) == typeof(Half)) return true;
            return !T.IsNegative(value)
                ? value <= T.CreateSaturating(9007199254740991L)
                : value >= T.CreateSaturating(-9007199254740991L);
        }

        public static bool isSafeInteger<T>(T? value) where T : struct, INumber<T> =>
            value.HasValue && isSafeInteger(value.Value);


        public static string toString<T>(this T value) where T : INumberBase<T> =>
            FormatDecimal(value);

        public static string toString<T>(this T? value) where T : struct, INumberBase<T> =>
            value.HasValue ? toString(value.Value) : string.Empty;

        public static string toString<T>(this T value, int radix) where T : IBinaryInteger<T>
        {
            if (radix < 2 || radix > 36) throw new RangeError("Number radix must be between 2 and 36.");
            var negative = T.IsNegative(value);
            var magnitude = negative
                ? checked(UInt128.CreateChecked(~value) + 1)
                : UInt128.CreateChecked(value);
            Span<char> buffer = stackalloc char[129];
            var position = buffer.Length;
            const string alphabet = "0123456789abcdefghijklmnopqrstuvwxyz";
            do
            {
                buffer[--position] = alphabet[(int)(magnitude % (uint)radix)];
                magnitude /= (uint)radix;
            } while (magnitude != 0);
            if (negative) buffer[--position] = '-';
            return new string(buffer[position..]);
        }

        public static string toString<T>(this T? value, int radix) where T : struct, IBinaryInteger<T> =>
            value.HasValue ? toString(value.Value, radix) : string.Empty;

        public static string toFixed<T>(this T value, int? digits = null) where T : INumberBase<T>
        {
            var count = digits ?? 0;
            if (count < 0 || count > 100) throw new RangeError("Number fraction digits must be between 0 and 100.");
            if (IsFloating<T>() && (!T.IsFinite(value) || System.Math.Abs(double.CreateChecked(value)) >= 1e21))
                return FormatDecimal(value);
            Span<char> output = stackalloc char[160];
            Span<char> format = stackalloc char[4];
            if (T.IsZero(value)) value = T.Zero;
            if (!value.TryFormat(output, out var length, PrecisionFormat('F', count, format), CultureInfo.InvariantCulture))
                throw new InvalidOperationException("Fixed numeric formatting exceeded its bounded buffer.");
            if (DecimalHalfRoundsDown(value, count)) output[length - 1]++;
            return new string(output[..length]);
        }

        public static string toExponential<T>(this T value, int? fractionDigits = null) where T : INumberBase<T>
        {
            if (fractionDigits < 0 || fractionDigits > 100) throw new RangeError("Number fraction digits must be between 0 and 100.");
            if (!T.IsFinite(value)) return FormatDecimal(value);
            return FormatSignificant(value, fractionDigits.HasValue ? fractionDigits.Value + 1 : null, true);
        }

        public static string toPrecision<T>(this T value, int? precision = null) where T : INumberBase<T>
        {
            if (precision < 1 || precision > 100) throw new RangeError("Number precision must be between 1 and 100.");
            if (!precision.HasValue || !T.IsFinite(value)) return FormatDecimal(value);
            return FormatSignificant(value, precision.Value, false);
        }

        public static string toLocaleString<T>(this T value, object? locales = null, object? options = null)
            where T : INumberBase<T> => FormatDecimal(value);

        public static T valueOf<T>(this T value) where T : INumberBase<T> => value;

        public static T? valueOf<T>(this T? value) where T : struct, INumberBase<T> => value;
    }
}
