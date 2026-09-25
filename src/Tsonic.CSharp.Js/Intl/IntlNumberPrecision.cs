using System;
using System.Globalization;
using System.Numerics;
using Tsonic.CSharp.Runtime;

namespace Tsonic.CSharp.Js
{
    internal sealed class IntlNumberPrecision
    {
        internal int MinimumInteger { get; }
        internal int? MinimumFraction { get; }
        internal int? MaximumFraction { get; }
        internal int? MinimumSignificant { get; }
        internal int? MaximumSignificant { get; }

        internal IntlNumberPrecision(TsValue options, int defaultFractionMinimum, int defaultFractionMaximum)
        {
            MinimumInteger = IntlRuntime.IntegerOption(options, "minimumIntegerDigits", 1, 21) ?? 1;
            var minimumSignificant = IntlRuntime.IntegerOption(options, "minimumSignificantDigits", 1, 21);
            var maximumSignificant = IntlRuntime.IntegerOption(options, "maximumSignificantDigits", 1, 21);
            if (minimumSignificant.HasValue || maximumSignificant.HasValue)
            {
                MinimumSignificant = minimumSignificant ?? 1;
                MaximumSignificant = maximumSignificant ?? 21;
                if (MinimumSignificant > MaximumSignificant) throw new RangeError("Minimum significant digits exceed maximum significant digits.");
            }
            else
            {
                var minimumFraction = IntlRuntime.IntegerOption(options, "minimumFractionDigits", 0, 100);
                var maximumFraction = IntlRuntime.IntegerOption(options, "maximumFractionDigits", 0, 100);
                MinimumFraction = minimumFraction ?? System.Math.Min(defaultFractionMinimum, maximumFraction ?? defaultFractionMaximum);
                MaximumFraction = maximumFraction ?? System.Math.Max(defaultFractionMaximum, MinimumFraction.Value);
                if (MinimumFraction > MaximumFraction) throw new RangeError("Minimum fraction digits exceed maximum fraction digits.");
            }
        }

        internal (string Integer, string Fraction) Format(string magnitude, bool percent)
        {
            var exponentIndex = magnitude.IndexOfAny(['e', 'E']);
            var exponent = exponentIndex < 0 ? 0 : int.Parse(magnitude[(exponentIndex + 1)..], CultureInfo.InvariantCulture);
            var mantissa = exponentIndex < 0 ? magnitude : magnitude[..exponentIndex];
            var point = mantissa.IndexOf('.');
            if (point >= 0) exponent -= mantissa.Length - point - 1;
            var coefficient = BigInteger.Parse(mantissa.Replace(".", ""), CultureInfo.InvariantCulture);
            if (percent) exponent += 2;
            if (coefficient.IsZero) exponent = 0;
            var digits = coefficient.ToString(CultureInfo.InvariantCulture);
            var quantum = MaximumSignificant.HasValue
                ? digits.Length + exponent - MaximumSignificant.Value
                : -MaximumFraction!.Value;
            if (quantum > exponent)
            {
                var divisor = BigInteger.Pow(10, quantum - exponent);
                coefficient = BigInteger.DivRem(coefficient, divisor, out var remainder);
                if (remainder * 2 >= divisor) coefficient++;
                exponent = quantum;
                digits = coefficient.ToString(CultureInfo.InvariantCulture);
            }
            var decimalPoint = digits.Length + exponent;
            string integer;
            string fraction;
            if (decimalPoint <= 0)
            {
                integer = "0";
                fraction = new string('0', -decimalPoint) + digits;
            }
            else if (decimalPoint >= digits.Length)
            {
                integer = digits + new string('0', decimalPoint - digits.Length);
                fraction = "";
            }
            else
            {
                integer = digits[..decimalPoint];
                fraction = digits[decimalPoint..];
            }
            fraction = fraction.TrimEnd('0');
            if (MinimumSignificant.HasValue)
            {
                var significant = (integer + fraction).TrimStart('0').Length;
                if (significant == 0) significant = 1;
                fraction += new string('0', System.Math.Max(0, MinimumSignificant.Value - significant));
            }
            else fraction = fraction.PadRight(MinimumFraction!.Value, '0');
            return (integer.PadLeft(MinimumInteger, '0'), fraction);
        }
    }
}
