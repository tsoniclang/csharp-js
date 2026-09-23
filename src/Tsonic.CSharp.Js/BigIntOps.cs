using System;
using System.Numerics;

namespace Tsonic.CSharp.Js;

public static class BigIntOps
{
    public static BigInteger from(sbyte value) => FromNumeric(value);
    public static BigInteger from(byte value) => FromNumeric(value);
    public static BigInteger from(short value) => FromNumeric(value);
    public static BigInteger from(ushort value) => FromNumeric(value);
    public static BigInteger from(int value) => FromNumeric(value);
    public static BigInteger from(uint value) => FromNumeric(value);
    public static BigInteger from(long value) => FromNumeric(value);
    public static BigInteger from(ulong value) => FromNumeric(value);
    public static BigInteger from(nint value) => FromNumeric(value);
    public static BigInteger from(nuint value) => FromNumeric(value);
    public static BigInteger from(Int128 value) => FromNumeric(value);
    public static BigInteger from(UInt128 value) => FromNumeric(value);
    public static BigInteger from(Half value) => FromNumeric(value);
    public static BigInteger from(float value) => FromNumeric(value);
    public static BigInteger from(double value) => FromNumeric(value);
    public static BigInteger from(decimal value) => FromNumeric(value);
    public static BigInteger from(BigInteger value) => FromNumeric(value);
    public static BigInteger from(bool value) => value ? BigInteger.One : BigInteger.Zero;
    public static BigInteger from(string value) => value is null
        ? throw new TypeError("BigInt requires a numeric, boolean or string value") : FromString(value);

    private static BigInteger FromNumeric<T>(T value) where T : INumberBase<T> =>
        T.IsInteger(value) ? BigInteger.CreateChecked(value)
            : throw new RangeError("The number cannot be converted to a BigInt because it is not an integer");

    public static string toString(BigInteger value, int radix = 10)
    {
        if (radix < 2 || radix > 36) throw new RangeError("BigInt radix must be between 2 and 36.");
        if (radix == 10 || value.IsZero) return value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var negative = value.Sign < 0;
        value = BigInteger.Abs(value);
        var digits = new System.Collections.Generic.List<char>();
        const string alphabet = "0123456789abcdefghijklmnopqrstuvwxyz";
        while (!value.IsZero)
        {
            value = BigInteger.DivRem(value, radix, out var remainder);
            digits.Add(alphabet[(int)remainder]);
        }
        if (negative) digits.Add('-');
        digits.Reverse();
        return new string(System.Runtime.InteropServices.CollectionsMarshal.AsSpan(digits));
    }

    public static Int128 AsIntNative<T>(double bits, T value) where T : IBinaryInteger<T>
    {
        var width = NativeWidth(bits);
        if (width == 0) return 0;
        var mask = NativeMask(width);
        var truncated = UInt128.CreateTruncating(value) & mask;
        return Int128.CreateTruncating((truncated & ((UInt128)1 << (width - 1))) != 0
            ? truncated | ~mask : truncated);
    }

    public static UInt128 AsUintNative<T>(double bits, T value) where T : IBinaryInteger<T> =>
        UInt128.CreateTruncating(value) & NativeMask(NativeWidth(bits));

    private static int NativeWidth(double bits)
    {
        if (!double.IsFinite(bits) || bits < 0 || bits > 128 || bits != System.Math.Truncate(bits))
            throw new RangeError("BigInt bit width must be an integer within the selected native result");
        return (int)bits;
    }

    private static UInt128 NativeMask(int width) =>
        width == 128 ? UInt128.MaxValue : ((UInt128)1 << width) - 1;

    public static BigInteger asIntN<T>(double bits, T value) where T : IBinaryInteger<T> =>
        TruncateBits(bits, value, true);

    public static BigInteger asUintN<T>(double bits, T value) where T : IBinaryInteger<T> =>
        TruncateBits(bits, value, false);

    private static BigInteger TruncateBits<T>(double bits, T value, bool signed) where T : IBinaryInteger<T>
    {
        bits = double.IsNaN(bits) ? 0 : System.Math.Truncate(bits);
        if (!double.IsFinite(bits) || bits < 0 || bits >= 18446744073709551616d || bits != System.Math.Truncate(bits))
            throw new RangeError("BigInt bit width must be a non-negative native integer");
        var width = (ulong)bits;
        if (width == 0) return BigInteger.Zero;
        if (width <= 128)
        {
            var count = (int)width;
            var mask = count == 128 ? UInt128.MaxValue : ((UInt128)1 << count) - 1;
            var truncated = UInt128.CreateTruncating(value) & mask;
            return signed && (truncated & ((UInt128)1 << (count - 1))) != 0
                ? BigInteger.CreateChecked(Int128.CreateTruncating(truncated | ~mask))
                : BigInteger.CreateChecked(truncated);
        }
        var integer = BigInteger.CreateChecked(value);
        if ((signed || integer.Sign >= 0) && width > (ulong)integer.GetBitLength()) return integer;
        if (width > int.MaxValue) throw new RangeError("BigInt result exceeds addressable storage");
        var modulus = BigInteger.One << (int)width;
        var result = integer & (modulus - BigInteger.One);
        return signed && result >= (modulus >> 1) ? result - modulus : result;
    }

    public static BigInteger from(object? value)
    {
        return value switch
        {
            BigInteger integer => integer,
            sbyte integer => new BigInteger(integer),
            byte integer => new BigInteger(integer),
            short integer => new BigInteger(integer),
            ushort integer => new BigInteger(integer),
            int integer => new BigInteger(integer),
            uint integer => new BigInteger(integer),
            long integer => new BigInteger(integer),
            ulong integer => new BigInteger(integer),
            nint integer => BigInteger.CreateChecked(integer),
            nuint integer => BigInteger.CreateChecked(integer),
            Int128 integer => BigInteger.CreateChecked(integer),
            UInt128 integer => BigInteger.CreateChecked(integer),
            bool boolean => boolean ? BigInteger.One : BigInteger.Zero,
            double number => FromNumber(number),
            float number => FromNumber(number),
            Half number => FromNumber((double)number),
            decimal number when decimal.Truncate(number) == number => new BigInteger(number),
            decimal => throw new RangeError("The number cannot be converted to a BigInt because it is not an integer"),
            string text => FromString(text),
            _ => throw new TypeError("BigInt requires a closed integer, number, boolean or string value"),
        };
    }

    public static BigInteger from<TFirst, TSecond>(Union<TFirst, TSecond> value) =>
        value.Match(part => from(part), part => from(part));

    public static BigInteger from<TFirst, TSecond, TThird>(Union<TFirst, TSecond, TThird> value) =>
        value.Match(part => from(part), part => from(part), part => from(part));

    public static BigInteger from<TFirst, TSecond, TThird, TFourth>(Union<TFirst, TSecond, TThird, TFourth> value) =>
        value.Match(part => from(part), part => from(part), part => from(part), part => from(part));

    public static BigInteger from<TFirst, TSecond, TThird, TFourth, TFifth>(Union<TFirst, TSecond, TThird, TFourth, TFifth> value) =>
        value.Match(part => from(part), part => from(part), part => from(part), part => from(part), part => from(part));

    public static BigInteger from<TFirst, TSecond, TThird, TFourth, TFifth, TSixth>(Union<TFirst, TSecond, TThird, TFourth, TFifth, TSixth> value) =>
        value.Match(part => from(part), part => from(part), part => from(part), part => from(part), part => from(part), part => from(part));

    public static BigInteger from<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh>(Union<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh> value) =>
        value.Match(part => from(part), part => from(part), part => from(part), part => from(part), part => from(part), part => from(part), part => from(part));

    public static BigInteger from<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth>(Union<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth> value) =>
        value.Match(part => from(part), part => from(part), part => from(part), part => from(part), part => from(part), part => from(part), part => from(part), part => from(part));

    private static BigInteger FromNumber(double value)
    {
        if (!double.IsFinite(value) || System.Math.Truncate(value) != value)
        {
            throw new RangeError("The number cannot be converted to a BigInt because it is not an integer");
        }
        return new BigInteger(value);
    }

    private static BigInteger FromString(string source)
    {
        var text = Number.TrimWhitespace(source.AsSpan());
        if (text.IsEmpty) return BigInteger.Zero;
        var radix = 10;
        if (text.Length >= 2 && text[0] == '0')
        {
            radix = text[1] switch { 'x' or 'X' => 16, 'o' or 'O' => 8, 'b' or 'B' => 2, _ => 10 };
            if (radix != 10) text = text[2..];
        }
        if (radix == 10)
        {
            return text[^1] is >= '0' and <= '9' &&
                BigInteger.TryParse(text, System.Globalization.NumberStyles.AllowLeadingSign,
                System.Globalization.CultureInfo.InvariantCulture, out var integer)
                ? integer
                : throw new SyntaxError("Cannot convert the string to a BigInt");
        }
        if (text.IsEmpty) throw new SyntaxError("Cannot convert the string to a BigInt");
        var result = BigInteger.Zero;
        foreach (var character in text)
        {
            var digit = character switch
            {
                >= '0' and <= '9' => character - '0',
                >= 'a' and <= 'f' => character - 'a' + 10,
                >= 'A' and <= 'F' => character - 'A' + 10,
                _ => -1,
            };
            if (digit < 0 || digit >= radix) throw new SyntaxError("Cannot convert the string to a BigInt");
            result = result * radix + digit;
        }
        return result;
    }
}
