using System;
using System.Collections.Generic;
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
            return [];

        if (value is JSObject jsObject)
        {
            return jsObject.entries().Select(entry => new KeyValuePair<string, object?>(entry.key, entry.value));
        }

        if (value is IDictionary<string, object?> dictionary)
            return dictionary;

        if (value is IReadOnlyDictionary<string, object?> readOnlyDictionary)
            return readOnlyDictionary;

        throw new NotSupportedException($"Object helpers require a closed JS object carrier, got '{value.GetType().FullName}'.");
    }

    public static string[] keys(object? value)
    {
        return Enumerate(value).Select(pair => pair.Key).ToArray();
    }

    public static object?[] values(object? value)
    {
        return Enumerate(value).Select(pair => pair.Value).ToArray();
    }

    public static (string key, object? value)[] entries(object? value)
    {
        return Enumerate(value).Select(pair => (pair.Key, pair.Value)).ToArray();
    }
}
