using System;
using System.Numerics;

namespace Tsonic.CSharp.Js;

public static class BigIntOps
{
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
        var start = 0;
        var end = source.Length;
        while (start < end && IsWhitespace(source[start])) start++;
        while (end > start && IsWhitespace(source[end - 1])) end--;
        var text = source.AsSpan(start, end - start);
        if (text.IsEmpty) return BigInteger.Zero;
        var radix = 10;
        if (text.Length >= 2 && text[0] == '0')
        {
            radix = text[1] switch { 'x' or 'X' => 16, 'o' or 'O' => 8, 'b' or 'B' => 2, _ => 10 };
            if (radix != 10) text = text[2..];
        }
        var negative = false;
        if (radix == 10 && !text.IsEmpty && text[0] is '+' or '-')
        {
            negative = text[0] == '-';
            text = text[1..];
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
        return negative ? -result : result;
    }

    private static bool IsWhitespace(char value) => value is
        '\u0009' or '\u000b' or '\u000c' or '\u0020' or '\u00a0' or '\ufeff' or
        '\u000a' or '\u000d' or '\u2028' or '\u2029' or '\u1680' or
        >= '\u2000' and <= '\u200a' or '\u202f' or '\u205f' or '\u3000';
}
