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
}
