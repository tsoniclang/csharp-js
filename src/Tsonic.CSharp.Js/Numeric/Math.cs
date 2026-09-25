/**
 * JavaScript Math namespace implementation
 */

using System;

namespace Tsonic.CSharp.Js
{
    /// <summary>
    /// Math namespace with JavaScript constants and functions
    /// </summary>
    public static class Math
    {
        // Mathematical constants
        public const double E = 2.718281828459045;
        public const double PI = 3.141592653589793;
        public const double LN2 = 0.6931471805599453;
        public const double LN10 = 2.302585092994046;
        public const double LOG2E = 1.4426950408889634;
        public const double LOG10E = 0.4342944819032518;
        public const double SQRT1_2 = 0.7071067811865476;
        public const double SQRT2 = 1.4142135623730951;

        // Common mathematical functions
        public static double abs(double x) => System.Math.Abs(x);
        public static double ceil(double x) => System.Math.Ceiling(x);
        public static double floor(double x) => System.Math.Floor(x);
        public static double round(double value)
        {
            if (!double.IsFinite(value) || value == 0) return value;
            var lower = System.Math.Floor(value);
            var rounded = value - lower >= 0.5 ? lower + 1 : lower;
            return rounded == 0 && value < 0 ? -0.0 : rounded;
        }
        public static double sqrt(double x) => System.Math.Sqrt(x);
        public static double pow(double x, double y) =>
            System.Math.Abs(x) == 1 && double.IsInfinity(y) ? double.NaN : System.Math.Pow(x, y);

        // Min/max with params
        public static double max(params double[] values)
        {
            var result = double.NegativeInfinity;
            foreach (var value in values) result = System.Math.Max(result, value);
            return result;
        }

        public static double min(params double[] values)
        {
            var result = double.PositiveInfinity;
            foreach (var value in values) result = System.Math.Min(result, value);
            return result;
        }

        // Trigonometric functions
        public static double sin(double x) => System.Math.Sin(x);
        public static double cos(double x) => System.Math.Cos(x);
        public static double tan(double x) => System.Math.Tan(x);
        public static double asin(double x) => System.Math.Asin(x);
        public static double acos(double x) => System.Math.Acos(x);
        public static double atan(double x) => System.Math.Atan(x);
        public static double atan2(double y, double x) => System.Math.Atan2(y, x);

        // Exponential and logarithmic
        public static double exp(double x) => System.Math.Exp(x);
        public static double log(double x) => System.Math.Log(x);
        public static double log10(double x) => System.Math.Log10(x);
        public static double log2(double x) => System.Math.Log2(x);

        // Random number generation
        private static readonly Random _random = new Random();
        public static double random() => _random.NextDouble();

        // Sign and truncation
        public static double sign(double value) =>
            double.IsNaN(value) || value == 0 ? value : value > 0 ? 1 : -1;
        public static double trunc(double x) => System.Math.Truncate(x);

        // Hyperbolic functions
        public static double sinh(double x) => System.Math.Sinh(x);
        public static double cosh(double x) => System.Math.Cosh(x);
        public static double tanh(double x) => System.Math.Tanh(x);
        public static double asinh(double x) => System.Math.Asinh(x);
        public static double acosh(double x) => System.Math.Acosh(x);
        public static double atanh(double x) => System.Math.Atanh(x);

        // Additional math functions
        public static double cbrt(double x) => System.Math.Cbrt(x);
        public static double hypot(params double[] values)
        {
            var maxAbs = 0.0;
            var hasNaN = false;
            foreach (var value in values)
            {
                var abs = System.Math.Abs(value);
                if (double.IsInfinity(abs))
                {
                    return double.PositiveInfinity;
                }

                if (double.IsNaN(abs))
                {
                    hasNaN = true;
                    continue;
                }

                maxAbs = System.Math.Max(maxAbs, abs);
            }

            if (hasNaN)
            {
                return double.NaN;
            }

            if (maxAbs == 0)
            {
                return 0;
            }

            var sum = 0.0;
            foreach (var value in values)
            {
                var scaled = value / maxAbs;
                sum += scaled * scaled;
            }

            return maxAbs * System.Math.Sqrt(sum);
        }

        public static double expm1(double x) => System.Math.Exp(x) - 1;
        public static double log1p(double x) => System.Math.Log(1 + x);

        // Floating point operations
        public static double fround(double x) => (double)(float)x;
        public static int imul<TLeft, TRight>(TLeft left, TRight right)
            where TLeft : System.Numerics.INumberBase<TLeft>
            where TRight : System.Numerics.INumberBase<TRight> =>
            unchecked((int)(NativeInteger.Bits32(left) * NativeInteger.Bits32(right)));

        public static int clz32<T>(T value) where T : System.Numerics.INumberBase<T> =>
            System.Numerics.BitOperations.LeadingZeroCount(NativeInteger.Bits32(value));

        // ES2024: Round to 16-bit float
        public static double f16round(double x)
        {
            // Convert to half precision (16-bit) and back
            var half = (Half)(float)x;
            return (double)half;
        }

    }
}
