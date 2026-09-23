using System;
using System.Globalization;
using System.Numerics;

namespace Tsonic.CSharp.Js
{
    public static class Number
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
            var selectedRadix = radix ?? 10;
            if (selectedRadix == 10)
                return Int128.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out var integer)
                    ? (double)integer : double.NaN;
            if (selectedRadix < 2 || selectedRadix > 36) return double.NaN;
            var source = str.AsSpan().Trim();
            var negative = source.Length > 0 && source[0] == '-';
            if (source.Length > 0 && (source[0] == '-' || source[0] == '+')) source = source[1..];
            if (source.IsEmpty) return double.NaN;
            var limit = negative ? (UInt128)Int128.MaxValue + 1 : (UInt128)Int128.MaxValue;
            var value = (UInt128)0;
            foreach (var character in source)
            {
                var digit = character switch
                {
                    >= '0' and <= '9' => character - '0',
                    >= 'a' and <= 'z' => character - 'a' + 10,
                    >= 'A' and <= 'Z' => character - 'A' + 10,
                    _ => -1,
                };
                if (digit < 0 || digit >= selectedRadix || value > (limit - (uint)digit) / (uint)selectedRadix)
                    return double.NaN;
                value = value * (uint)selectedRadix + (uint)digit;
            }
            var result = negative ? unchecked(-(Int128)value) : (Int128)value;
            return (double)result;
        }

        public static double parseFloat(string str) =>
            double.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out var result)
                ? result : double.NaN;

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

        public static bool isSafeInteger<T>(T value) where T : System.Numerics.INumberBase<T>
        {
            if (!T.IsInteger(value)) return false;
            if (typeof(T) == typeof(double)) return System.Math.Abs(double.CreateChecked(value)) <= MAX_SAFE_INTEGER;
            if (typeof(T) == typeof(float)) return System.MathF.Abs(float.CreateChecked(value)) <= 16777215f;
            if (typeof(T) == typeof(Half)) return Half.Abs(Half.CreateChecked(value)) <= (Half)2047;
            return true;
        }

        public static bool isSafeInteger<T>(T? value) where T : struct, System.Numerics.INumberBase<T> =>
            value.HasValue && isSafeInteger(value.Value);


        public static string toString<T>(this T value) where T : INumberBase<T> =>
            value.ToString(null, CultureInfo.InvariantCulture);

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
            if (count < 0) throw new RangeError("Number fraction digits must be non-negative.");
            return value.ToString($"F{count}", CultureInfo.InvariantCulture);
        }

        public static string toExponential<T>(this T value, int? fractionDigits = null) where T : INumberBase<T>
        {
            if (fractionDigits < 0) throw new RangeError("Number fraction digits must be non-negative.");
            var format = fractionDigits.HasValue ? $"E{fractionDigits.Value}" : "E";
            return value.ToString(format, CultureInfo.InvariantCulture);
        }

        public static string toPrecision<T>(this T value, int? precision = null) where T : INumberBase<T>
        {
            if (precision < 1) throw new RangeError("Number precision must be positive.");
            var format = precision.HasValue ? $"G{precision.Value}" : "G";
            return value.ToString(format, CultureInfo.InvariantCulture);
        }

        public static string toLocaleString<T>(this T value, object? locales = null, object? options = null)
            where T : INumberBase<T> => value.ToString(null, CultureInfo.InvariantCulture);

        public static T valueOf<T>(this T value) where T : INumberBase<T> => value;

        public static T? valueOf<T>(this T? value) where T : struct, INumberBase<T> => value;
    }
}
