using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;

namespace Tsonic.CSharp.Js;

/// <summary>
/// JavaScript Object static helpers.
/// </summary>
public static class Object
{
    public static bool @is(object? value, object? other)
    {
        return SameValue(UnwrapClosedValue(value), UnwrapClosedValue(other));
    }

    public static bool @is<TValue, TOther>(TValue value, TOther other)
    {
        return value is TsValue or IClosedUnionValue || other is TsValue or IClosedUnionValue
            ? @is((object?)value, (object?)other)
            : SameValue(value, other);
    }

    private static IEnumerable<KeyValuePair<string, object?>> Enumerate(object? value)
    {
        value = UnwrapClosedValue(value);
        if (value == null)
        {
            throw new TypeError("Object helper receiver cannot be null.");
        }

        if (value is JSObject jsObject)
        {
            return jsObject.entries().Select(entry => new KeyValuePair<string, object?>(entry.key, entry.value));
        }

        if (value is TsObject tsObject)
        {
            return tsObject.entries();
        }

        if (value is TsArray tsArray)
        {
            return tsArray.entries();
        }

        if (value is IDynamicArray jsArray)
        {
            return jsArray.Entries();
        }

        if (value is IDictionary<string, TsValue> record)
            return record.Select(static pair => new KeyValuePair<string, object?>(pair.Key, pair.Value.unwrap()));

        if (value is IReadOnlyDictionary<string, TsValue> readOnlyRecord)
            return readOnlyRecord.Select(static pair => new KeyValuePair<string, object?>(pair.Key, pair.Value.unwrap()));

        if (value is IDictionary<string, object?> dictionary)
            return dictionary;

        if (value is IReadOnlyDictionary<string, object?> readOnlyDictionary)
            return readOnlyDictionary;

        if (value is string text)
        {
            return EnumerateString(text);
        }

        if (HasNoEnumerableOwnProperties(value))
        {
            return [];
        }

        throw new NotSupportedException("Object helpers require a closed JS object carrier.");
    }

    public static JSArray<string> keys(object? value)
    {
        var result = new JSArray<string>();
        foreach (var pair in Enumerate(value))
        {
            result.push(pair.Key);
        }

        return result;
    }

    public static JSArray<string> keys<TValue>(Dictionary<string, TValue> value)
    {
        return keys((IReadOnlyDictionary<string, TValue>)value);
    }

    public static JSArray<string> keys<TValue>(IDictionary<string, TValue> value)
    {
        var result = new JSArray<string>();
        foreach (var pair in EnumerateDictionary(value))
        {
            result.push(pair.Key);
        }

        return result;
    }

    public static JSArray<string> keys<TValue>(IReadOnlyDictionary<string, TValue> value)
    {
        var result = new JSArray<string>();
        foreach (var pair in EnumerateDictionary(value))
        {
            result.push(pair.Key);
        }

        return result;
    }

    public static JSArray<object?> values(object? value)
    {
        var result = new JSArray<object?>();
        foreach (var pair in Enumerate(value))
        {
            result.push(pair.Value);
        }

        return result;
    }

    public static JSArray<TValue> values<TValue>(Dictionary<string, TValue> value)
    {
        return values((IReadOnlyDictionary<string, TValue>)value);
    }

    public static JSArray<TValue> values<TValue>(IDictionary<string, TValue> value)
    {
        var result = new JSArray<TValue>();
        foreach (var pair in EnumerateDictionary(value))
        {
            result.push(pair.Value);
        }

        return result;
    }

    public static JSArray<TValue> values<TValue>(IReadOnlyDictionary<string, TValue> value)
    {
        var result = new JSArray<TValue>();
        foreach (var pair in EnumerateDictionary(value))
        {
            result.push(pair.Value);
        }

        return result;
    }

    public static JSArray<(string key, object? value)> entries(object? value)
    {
        var result = new JSArray<(string key, object? value)>();
        foreach (var pair in Enumerate(value))
        {
            result.push((pair.Key, pair.Value));
        }

        return result;
    }

    public static JSArray<(string key, TValue value)> entries<TValue>(Dictionary<string, TValue> value)
    {
        return entries((IReadOnlyDictionary<string, TValue>)value);
    }

    public static JSArray<(string key, TValue value)> entries<TValue>(IDictionary<string, TValue> value)
    {
        var result = new JSArray<(string key, TValue value)>();
        foreach (var pair in EnumerateDictionary(value))
        {
            result.push((pair.Key, pair.Value));
        }

        return result;
    }

    public static JSArray<(string key, TValue value)> entries<TValue>(IReadOnlyDictionary<string, TValue> value)
    {
        var result = new JSArray<(string key, TValue value)>();
        foreach (var pair in EnumerateDictionary(value))
        {
            result.push((pair.Key, pair.Value));
        }

        return result;
    }

    public static bool hasOwn(JSObject value, string key)
    {
        if (value == null)
        {
            throw new TypeError("Object.hasOwn receiver cannot be null.");
        }

        return value.hasOwnProperty(key);
    }

    public static string toString(object? value)
    {
        value = UnwrapClosedValue(value);
        return value switch
        {
            null => "[object Null]",
            string text => text,
            bool boolean => BooleanOps.toString(boolean),
            byte number => Number.toString(number),
            sbyte number => Number.toString(number),
            short number => Number.toString(number),
            ushort number => Number.toString(number),
            int number => Number.toString(number),
            uint number => Number.toString(number),
            long number => Number.toString(number),
            ulong number => Number.toString(number),
            float number => Number.toString(number),
            double number => Number.toString(number),
            decimal number => Number.toString((double)number),
            Date date => date.ToString(),
            RegExp regexp => regexp.ToString(),
            IDynamicArray => "[object Array]",
            _ => "[object Object]",
        };
    }

    public static bool hasOwn(IDynamicArray value, string key)
    {
        if (value == null)
        {
            throw new TypeError("Object.hasOwn receiver cannot be null.");
        }

        return value.HasOwn(key);
    }

    public static bool hasOwn(string value, string key)
    {
        if (value == null)
        {
            throw new TypeError("Object.hasOwn receiver cannot be null.");
        }

        return TryParseArrayIndex(key, value.Length, out _);
    }

    public static bool hasOwn<TValue>(Dictionary<string, TValue> value, string key)
    {
        return hasOwn((IReadOnlyDictionary<string, TValue>)value, key);
    }

    public static bool hasOwn<TValue>(IReadOnlyDictionary<string, TValue> value, string key)
    {
        if (value == null)
        {
            throw new TypeError("Object.hasOwn receiver cannot be null.");
        }

        return value.ContainsKey(key);
    }

    public static JSObject assign(JSObject target, params object?[] sources)
    {
        if (target == null)
        {
            throw new TypeError("Object.assign target cannot be null.");
        }

        foreach (var source in sources)
        {
            if (IsNullishAssignSource(source))
            {
                continue;
            }

            foreach (var pair in Enumerate(source))
            {
                target[pair.Key] = pair.Value;
            }
        }

        return target;
    }

    public static Dictionary<string, TValue> assign<TValue>(
        Dictionary<string, TValue> target,
        params IReadOnlyDictionary<string, TValue>[] sources
    )
    {
        if (target == null)
        {
            throw new TypeError("Object.assign target cannot be null.");
        }

        foreach (var source in sources)
        {
            if (IsNullishAssignSource(source))
            {
                continue;
            }

            foreach (var pair in source)
            {
                target[pair.Key] = pair.Value;
            }
        }

        return target;
    }

    public static IDictionary<string, object?> assign(IDictionary<string, object?> target, params object?[] sources)
    {
        if (target == null)
        {
            throw new TypeError("Object.assign target cannot be null.");
        }

        foreach (var source in sources)
        {
            if (source == null)
            {
                continue;
            }

            foreach (var pair in Enumerate(source))
            {
                target[pair.Key] = pair.Value;
            }
        }

        return target;
    }

    private static IEnumerable<KeyValuePair<string, TValue>> EnumerateDictionary<TValue>(
        IEnumerable<KeyValuePair<string, TValue>> dictionary
    )
    {
        if (dictionary == null)
        {
            throw new TypeError("Object helper receiver cannot be null.");
        }

        return dictionary;
    }

    private static bool IsNullishAssignSource(object? source)
    {
        var value = UnwrapClosedValue(source);
        return value is null;
    }

    private static object? UnwrapClosedValue(object? value)
    {
        return TsValue.UnwrapClosedValue(value);
    }

    private static IEnumerable<KeyValuePair<string, object?>> EnumerateString(string text)
    {
        for (var index = 0; index < text.Length; index++)
        {
            yield return new KeyValuePair<string, object?>(
                index.ToString(CultureInfo.InvariantCulture),
                text[index].ToString()
            );
        }
    }

    private static bool TryParseArrayIndex(string key, int length, out int index)
    {
        if (!int.TryParse(key, NumberStyles.None, CultureInfo.InvariantCulture, out index))
        {
            return false;
        }

        return index >= 0 &&
            index < length &&
            key == index.ToString(CultureInfo.InvariantCulture);
    }

    private static bool HasNoEnumerableOwnProperties(object value)
    {
        return value is bool
            or byte
            or sbyte
            or short
            or ushort
            or int
            or uint
            or long
            or ulong
            or float
            or double
            or decimal
            or char;
    }

    private static bool SameValue<TValue, TOther>(TValue value, TOther other) => value switch
    {
        null => other is null,
        byte number => SameNumericOther(number, other),
        sbyte number => SameNumericOther(number, other),
        short number => SameNumericOther(number, other),
        ushort number => SameNumericOther(number, other),
        int number => SameNumericOther(number, other),
        uint number => SameNumericOther(number, other),
        long number => SameNumericOther(number, other),
        ulong number => SameNumericOther(number, other),
        nint number => SameNumericOther(number, other),
        nuint number => SameNumericOther(number, other),
        Int128 number => SameNumericOther(number, other),
        UInt128 number => SameNumericOther(number, other),
        Half number => SameNumericOther(number, other),
        float number => SameNumericOther(number, other),
        double number => SameNumericOther(number, other),
        decimal number => SameNumericOther(number, other),
        BigInteger number => SameNumericOther(number, other),
        string text => other is string otherText && string.Equals(text, otherText, StringComparison.Ordinal),
        bool boolean => other is bool otherBoolean && boolean == otherBoolean,
        char character => other is char otherCharacter && character == otherCharacter,
        _ => ReferenceEquals(value, other),
    };

    private static bool SameNumericOther<TValue, TOther>(TValue value, TOther other)
        where TValue : INumberBase<TValue> => other switch
    {
        byte number => SameNumericValue(value, number),
        sbyte number => SameNumericValue(value, number),
        short number => SameNumericValue(value, number),
        ushort number => SameNumericValue(value, number),
        int number => SameNumericValue(value, number),
        uint number => SameNumericValue(value, number),
        long number => SameNumericValue(value, number),
        ulong number => SameNumericValue(value, number),
        nint number => SameNumericValue(value, number),
        nuint number => SameNumericValue(value, number),
        Int128 number => SameNumericValue(value, number),
        UInt128 number => SameNumericValue(value, number),
        Half number => SameNumericValue(value, number),
        float number => SameNumericValue(value, number),
        double number => SameNumericValue(value, number),
        decimal number => SameNumericValue(value, number),
        BigInteger number => SameNumericValue(value, number),
        _ => false,
    };

    private static bool SameNumericValue<TValue, TOther>(TValue value, TOther other)
        where TValue : INumberBase<TValue>
        where TOther : INumberBase<TOther>
    {
        if (typeof(TValue) == typeof(TOther))
        {
            var right = TValue.CreateChecked(other);
            if (!IsBinaryFloating<TValue>()) return value == right;
            return value == right
                ? !TValue.IsZero(value) || TValue.IsNegative(value) == TValue.IsNegative(right)
                : TValue.IsNaN(value) && TValue.IsNaN(right);
        }
        if (IsBinaryFloating<TValue>() && IsBinaryFloating<TOther>())
            return SameNumericValue(double.CreateChecked(value), double.CreateChecked(other));
        if (IsFixedInteger<TValue>() && IsFixedInteger<TOther>())
            return TValue.IsNegative(value) == TOther.IsNegative(other) && Magnitude(value) == Magnitude(other);
        if (TValue.IsNaN(value) || TOther.IsNaN(other)) return TValue.IsNaN(value) && TOther.IsNaN(other);
        if (TValue.IsZero(value) || TOther.IsZero(other))
            return TValue.IsZero(value) && TOther.IsZero(other) &&
                (IsBinaryFloating<TValue>() && TValue.IsNegative(value)) ==
                (IsBinaryFloating<TOther>() && TOther.IsNegative(other));
        if (TValue.IsNegative(value) != TOther.IsNegative(other)) return false;
        if (TValue.IsInfinity(value) || TOther.IsInfinity(other)) return TValue.IsInfinity(value) && TOther.IsInfinity(other);
        if (typeof(TValue) == typeof(BigInteger)) return SameBigInteger(BigInteger.CreateChecked(value), NumericParts(other));
        if (typeof(TOther) == typeof(BigInteger)) return SameBigInteger(BigInteger.CreateChecked(other), NumericParts(value));
        var leftParts = NumericParts(value);
        var rightParts = NumericParts(other);
        return leftParts == rightParts;
    }

    private static bool IsBinaryFloating<TValue>() => typeof(TValue) == typeof(Half) ||
        typeof(TValue) == typeof(float) || typeof(TValue) == typeof(double);

    private static bool IsFixedInteger<TValue>() => !IsBinaryFloating<TValue>() &&
        typeof(TValue) != typeof(decimal) && typeof(TValue) != typeof(BigInteger);

    private static UInt128 Magnitude<TValue>(TValue value) where TValue : INumberBase<TValue>
    {
        if (!TValue.IsNegative(value)) return UInt128.CreateChecked(value);
        var signed = Int128.CreateChecked(value);
        return UInt128.CreateChecked(-(signed + 1)) + 1;
    }

    private static (UInt128 Coefficient, int BinaryExponent, int FiveExponent) NumericParts<TValue>(TValue value)
        where TValue : INumberBase<TValue>
    {
        UInt128 coefficient;
        var binaryExponent = 0;
        var fiveExponent = 0;
        if (typeof(TValue) == typeof(decimal))
        {
            Span<int> bits = stackalloc int[4];
            decimal.GetBits(decimal.CreateChecked(value), bits);
            coefficient = (uint)bits[0] | ((UInt128)(uint)bits[1] << 32) | ((UInt128)(uint)bits[2] << 64);
            binaryExponent = fiveExponent = -((bits[3] >> 16) & 255);
        }
        else if (IsBinaryFloating<TValue>())
        {
            var bits = BitConverter.DoubleToUInt64Bits(double.CreateChecked(value));
            var exponent = (int)((bits >> 52) & 2047);
            coefficient = bits & 0x000F_FFFF_FFFF_FFFF;
            if (exponent != 0) coefficient |= (UInt128)1 << 52;
            binaryExponent = exponent == 0 ? -1074 : exponent - 1075;
        }
        else coefficient = Magnitude(value);
        while (coefficient % 2 == 0) { coefficient /= 2; binaryExponent++; }
        while (coefficient % 5 == 0) { coefficient /= 5; fiveExponent++; }
        return (coefficient, binaryExponent, fiveExponent);
    }

    private static bool SameBigInteger(BigInteger value, (UInt128 Coefficient, int BinaryExponent, int FiveExponent) parts)
    {
        if (parts.BinaryExponent < 0 || parts.FiveExponent < 0 || value.GetByteCount() > 129) return false;
        Span<byte> actual = stackalloc byte[129];
        Span<byte> expected = stackalloc byte[129];
        actual.Clear();
        expected.Clear();
        if (!value.TryWriteBytes(actual, out var written)) return false;
        if (value.Sign < 0)
        {
            var carry = 1;
            for (var index = 0; index < written; index++)
            {
                var digit = (byte)~actual[index] + carry;
                actual[index] = (byte)digit;
                carry = digit >> 8;
            }
        }
        var coefficient = parts.Coefficient;
        for (var index = 0; index < parts.FiveExponent; index++) coefficient *= 5;
        var offset = parts.BinaryExponent / 8;
        var shift = parts.BinaryExponent % 8;
        while (coefficient != 0)
        {
            var digit = (byte)coefficient;
            expected[offset] |= (byte)(digit << shift);
            if (shift != 0) expected[offset + 1] |= (byte)(digit >> (8 - shift));
            coefficient >>= 8;
            offset++;
        }
        return actual.SequenceEqual(expected);
    }
}
