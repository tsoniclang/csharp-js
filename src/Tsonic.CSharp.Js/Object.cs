using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Tsonic.CSharp.Js;

/// <summary>
/// JavaScript Object static helpers.
/// </summary>
public static class Object
{
    public static bool @is(object? value, object? other)
    {
        if (value == null || other == null)
        {
            return value == null && other == null;
        }

        if (tryGetNumericValue(value, out var left) && tryGetNumericValue(other, out var right))
        {
            if (double.IsNaN(left) && double.IsNaN(right))
            {
                return true;
            }

            if (left == 0 && right == 0)
            {
                return double.IsNegative(left) == double.IsNegative(right);
            }

            return left.Equals(right);
        }

        return ReferenceEquals(value, other) || value.Equals(other);
    }

    private static IEnumerable<KeyValuePair<string, object?>> Enumerate(object? value)
    {
        if (value == null)
        {
            throw new TypeError("Object helper receiver cannot be null.");
        }

        if (value is JSObject jsObject)
        {
            return jsObject.entries().Select(entry => new KeyValuePair<string, object?>(entry.key, entry.value));
        }

        if (value is IJSArray jsArray)
        {
            return EnumerateJsArray(jsArray);
        }

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

    public static List<string> keys(object? value)
    {
        var result = new List<string>();
        foreach (var pair in Enumerate(value))
        {
            result.Add(pair.Key);
        }

        return result;
    }

    public static List<string> keys<TValue>(Dictionary<string, TValue> value)
    {
        return keys((IReadOnlyDictionary<string, TValue>)value);
    }

    public static List<string> keys<TValue>(IDictionary<string, TValue> value)
    {
        var result = new List<string>();
        foreach (var pair in EnumerateDictionary(value))
        {
            result.Add(pair.Key);
        }

        return result;
    }

    public static List<string> keys<TValue>(IReadOnlyDictionary<string, TValue> value)
    {
        var result = new List<string>();
        foreach (var pair in EnumerateDictionary(value))
        {
            result.Add(pair.Key);
        }

        return result;
    }

    public static List<object?> values(object? value)
    {
        var result = new List<object?>();
        foreach (var pair in Enumerate(value))
        {
            result.Add(pair.Value);
        }

        return result;
    }

    public static List<TValue> values<TValue>(Dictionary<string, TValue> value)
    {
        return values((IReadOnlyDictionary<string, TValue>)value);
    }

    public static List<TValue> values<TValue>(IDictionary<string, TValue> value)
    {
        var result = new List<TValue>();
        foreach (var pair in EnumerateDictionary(value))
        {
            result.Add(pair.Value);
        }

        return result;
    }

    public static List<TValue> values<TValue>(IReadOnlyDictionary<string, TValue> value)
    {
        var result = new List<TValue>();
        foreach (var pair in EnumerateDictionary(value))
        {
            result.Add(pair.Value);
        }

        return result;
    }

    public static List<(string key, object? value)> entries(object? value)
    {
        var result = new List<(string key, object? value)>();
        foreach (var pair in Enumerate(value))
        {
            result.Add((pair.Key, pair.Value));
        }

        return result;
    }

    public static List<(string key, TValue value)> entries<TValue>(Dictionary<string, TValue> value)
    {
        return entries((IReadOnlyDictionary<string, TValue>)value);
    }

    public static List<(string key, TValue value)> entries<TValue>(IDictionary<string, TValue> value)
    {
        var result = new List<(string key, TValue value)>();
        foreach (var pair in EnumerateDictionary(value))
        {
            result.Add((pair.Key, pair.Value));
        }

        return result;
    }

    public static List<(string key, TValue value)> entries<TValue>(IReadOnlyDictionary<string, TValue> value)
    {
        var result = new List<(string key, TValue value)>();
        foreach (var pair in EnumerateDictionary(value))
        {
            result.Add((pair.Key, pair.Value));
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

    public static bool hasOwn(IJSArray value, string key)
    {
        if (value == null)
        {
            throw new TypeError("Object.hasOwn receiver cannot be null.");
        }

        return TryParseArrayIndex(key, value.length, out var index) &&
            value.tryGetAtObject(index, out _);
    }

    public static bool hasOwn(string value, string key)
    {
        if (value == null)
        {
            throw new TypeError("Object.hasOwn receiver cannot be null.");
        }

        return TryParseArrayIndex(key, value.Length, out _);
    }

    public static JSObject assign(JSObject target, params object?[] sources)
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

    private static IEnumerable<KeyValuePair<string, object?>> EnumerateJsArray(IJSArray array)
    {
        for (var index = 0; index < array.length; index++)
        {
            if (array.tryGetAtObject(index, out var value))
            {
                yield return new KeyValuePair<string, object?>(
                    index.ToString(CultureInfo.InvariantCulture),
                    value
                );
            }
        }
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

    private static bool tryGetNumericValue(object value, out double result)
    {
        switch (value)
        {
            case byte typed:
                result = typed;
                return true;
            case sbyte typed:
                result = typed;
                return true;
            case short typed:
                result = typed;
                return true;
            case ushort typed:
                result = typed;
                return true;
            case int typed:
                result = typed;
                return true;
            case uint typed:
                result = typed;
                return true;
            case long typed:
                result = typed;
                return true;
            case ulong typed:
                result = typed;
                return true;
            case float typed:
                result = typed;
                return true;
            case double typed:
                result = typed;
                return true;
            case decimal typed:
                result = (double)typed;
                return true;
            default:
                result = 0;
                return false;
        }
    }
}
