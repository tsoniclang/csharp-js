using System;
using System.Globalization;

/**
 * JavaScript Number static methods
 * Provides Number.parseInt, Number.parseFloat, Number.isNaN, Number.isFinite, Number.isInteger
 */

namespace Tsonic.CSharp.Js
{
    /// <summary>
    /// JavaScript Number object static methods.
    /// Note: Number.parseInt and Number.parseFloat are aliases for the global functions.
    /// </summary>
    public static class Number
    {
        // Number constants
        public const double MAX_VALUE = double.MaxValue;
        public const double MIN_VALUE = double.Epsilon; // Smallest positive value
        public const double MAX_SAFE_INTEGER = 9007199254740991; // 2^53 - 1
        public const double MIN_SAFE_INTEGER = -9007199254740991; // -(2^53 - 1)
        public const double POSITIVE_INFINITY = double.PositiveInfinity;
        public const double NEGATIVE_INFINITY = double.NegativeInfinity;
        public const double NaN = double.NaN;
        public const double EPSILON = 2.220446049250313e-16; // 2^-52

        /// <summary>
        /// Parse string to integer with optional radix.
        /// Alias for the global parseInt function.
        /// </summary>
        public static double parseInt(string str, int? radix = null)
        {
            return Globals.parseInt(str, radix);
        }

        /// <summary>
        /// Parse string to floating point number.
        /// Alias for the global parseFloat function.
        /// </summary>
        public static double parseFloat(string str)
        {
            return Globals.parseFloat(str);
        }

        /// <summary>
        /// Check if value is NaN.
        /// Unlike global isNaN, this does NOT convert the argument.
        /// </summary>
        public static bool isNaN(double value)
        {
            return double.IsNaN(value);
        }

        public static bool isNaN(int value)
        {
            return false;
        }

        public static bool isNaN(int? value)
        {
            return value.HasValue && isNaN(value.Value);
        }

        public static bool isNaN(long value)
        {
            return false;
        }

        public static bool isNaN(long? value)
        {
            return value.HasValue && isNaN(value.Value);
        }

        /// <summary>
        /// Check if value is finite (not infinite or NaN).
        /// Unlike global isFinite, this does NOT convert the argument.
        /// </summary>
        public static bool isFinite(double value)
        {
            return !double.IsInfinity(value) && !double.IsNaN(value);
        }

        public static bool isFinite(int value)
        {
            return true;
        }

        public static bool isFinite(int? value)
        {
            return value.HasValue;
        }

        public static bool isFinite(long value)
        {
            return true;
        }

        public static bool isFinite(long? value)
        {
            return value.HasValue;
        }

        /// <summary>
        /// Check if value is an integer (no fractional part).
        /// </summary>
        public static bool isInteger(double value)
        {
            if (double.IsInfinity(value) || double.IsNaN(value))
            {
                return false;
            }
            return System.Math.Floor(value) == value;
        }

        public static bool isInteger(int value)
        {
            return true;
        }

        public static bool isInteger(int? value)
        {
            return value.HasValue;
        }

        public static bool isInteger(long value)
        {
            return true;
        }

        public static bool isInteger(long? value)
        {
            return value.HasValue;
        }

        /// <summary>
        /// Check if value is a safe integer (can be exactly represented).
        /// Safe integers are integers in the range -(2^53-1) to 2^53-1.
        /// </summary>
        public static bool isSafeInteger(double value)
        {
            if (!isInteger(value))
            {
                return false;
            }
            return value >= MIN_SAFE_INTEGER && value <= MAX_SAFE_INTEGER;
        }

        public static bool isSafeInteger(int value)
        {
            return true;
        }

        public static bool isSafeInteger(int? value)
        {
            return value.HasValue;
        }

        public static bool isSafeInteger(long value)
        {
            return value >= MIN_SAFE_INTEGER && value <= MAX_SAFE_INTEGER;
        }

        public static bool isSafeInteger(long? value)
        {
            return value.HasValue && isSafeInteger(value.Value);
        }

        /// <summary>
        /// Convert a number to its JavaScript string form.
        /// </summary>
        public static string toString(this double value)
        {
            return formatJsNumber(value);
        }

        public static string toString(this double? value)
        {
            return value.HasValue ? formatJsNumber(value.Value) : string.Empty;
        }

        public static string toString(this int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        public static string toString(this int value, int radix)
        {
            return formatSignedIntegralRadix(value, radix);
        }

        public static string toString(this int? value)
        {
            return value?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        }

        public static string toString(this int? value, int radix)
        {
            return value.HasValue ? formatSignedIntegralRadix(value.Value, radix) : string.Empty;
        }

        public static string toString(this long value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        public static string toString(this long value, int radix)
        {
            return formatSignedIntegralRadix(value, radix);
        }

        public static string toString(this long? value)
        {
            return value?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        }

        public static string toString(this long? value, int radix)
        {
            return value.HasValue ? formatSignedIntegralRadix(value.Value, radix) : string.Empty;
        }

        public static string toExponential(this double value, int? fractionDigits = null)
        {
            var special = formatSpecialJsNumber(value);
            if (special != null)
            {
                return special;
            }

            if (fractionDigits.HasValue)
            {
                validateFractionDigits(fractionDigits.Value);
                return normalizeExponent(value.ToString($"E{fractionDigits.Value}", CultureInfo.InvariantCulture));
            }

            return normalizeExponent(trimMantissaZeros(value.ToString("E15", CultureInfo.InvariantCulture)));
        }

        public static string toFixed(this double value, int? digits = null)
        {
            var special = formatSpecialJsNumber(value);
            if (special != null)
            {
                return special;
            }

            var fractionDigits = digits ?? 0;
            validateFractionDigits(fractionDigits);
            if (System.Math.Abs(value) >= 1e21)
            {
                return formatJsNumber(value);
            }

            return value.ToString($"F{fractionDigits}", CultureInfo.InvariantCulture);
        }

        public static string toPrecision(this double value, int? precision = null)
        {
            var special = formatSpecialJsNumber(value);
            if (special != null)
            {
                return special;
            }

            if (!precision.HasValue)
            {
                return formatJsNumber(value);
            }

            validatePrecision(precision.Value);
            return normalizeExponent(value.ToString($"G{precision.Value}", CultureInfo.InvariantCulture));
        }

        public static string toLocaleString(this double value, object? locales = null, object? options = null)
        {
            _ = locales;
            _ = options;
            return formatJsNumber(value);
        }

        /// <summary>
        /// Return the primitive numeric value.
        /// </summary>
        public static double valueOf(this double value)
        {
            return value;
        }

        public static double? valueOf(this double? value)
        {
            return value;
        }

        public static int valueOf(this int value)
        {
            return value;
        }

        public static int? valueOf(this int? value)
        {
            return value;
        }

        public static long valueOf(this long value)
        {
            return value;
        }

        public static long? valueOf(this long? value)
        {
            return value;
        }

        private static string formatJsNumber(double value)
        {
            var special = formatSpecialJsNumber(value);
            if (special != null)
            {
                return special;
            }

            return value == 0 ? "0" : value.ToString(CultureInfo.InvariantCulture);
        }

        private static string? formatSpecialJsNumber(double value)
        {
            if (double.IsNaN(value))
            {
                return "NaN";
            }

            if (double.IsPositiveInfinity(value))
            {
                return "Infinity";
            }

            return double.IsNegativeInfinity(value) ? "-Infinity" : null;
        }

        private static string formatSignedIntegralRadix(long value, int radix)
        {
            validateRadix(radix);
            if (value == 0)
            {
                return "0";
            }
            var negative = value < 0;
            var magnitude = negative
                ? (ulong)(-(value + 1)) + 1
                : (ulong)value;
            var digits = formatUnsignedIntegralRadix(magnitude, radix);
            return negative ? "-" + digits : digits;
        }

        private static string formatUnsignedIntegralRadix(ulong value, int radix)
        {
            const string alphabet = "0123456789abcdefghijklmnopqrstuvwxyz";
            Span<char> buffer = stackalloc char[64];
            var index = buffer.Length;
            var divisor = (ulong)radix;
            while (value > 0)
            {
                var digit = (int)(value % divisor);
                buffer[--index] = alphabet[digit];
                value /= divisor;
            }
            return new string(buffer[index..]);
        }

        private static void validateRadix(int value)
        {
            if (value < 2 || value > 36)
            {
                throw new RangeError("Number radix must be between 2 and 36.");
            }
        }

        private static void validateFractionDigits(int value)
        {
            if (value < 0 || value > 100)
            {
                throw new RangeError("Number fraction digits must be between 0 and 100.");
            }
        }

        private static void validatePrecision(int value)
        {
            if (value < 1 || value > 100)
            {
                throw new RangeError("Number precision must be between 1 and 100.");
            }
        }

        private static string trimMantissaZeros(string value)
        {
            var exponentIndex = value.IndexOf('E');
            if (exponentIndex < 0)
            {
                return value;
            }

            var mantissa = value.Substring(0, exponentIndex);
            mantissa = mantissa.Contains('.')
                ? mantissa.TrimEnd('0').TrimEnd('.')
                : mantissa;
            return mantissa + value.Substring(exponentIndex);
        }

        private static string normalizeExponent(string value)
        {
            var exponentIndex = value.IndexOf('E');
            if (exponentIndex < 0)
            {
                return value;
            }

            var mantissa = value.Substring(0, exponentIndex);
            var exponent = int.Parse(value.Substring(exponentIndex + 1), CultureInfo.InvariantCulture);
            return $"{mantissa}e{(exponent >= 0 ? "+" : "")}{exponent.ToString(CultureInfo.InvariantCulture)}";
        }
    }
}
