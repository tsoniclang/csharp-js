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

        throw new NotSupportedException("Object helpers require a closed JS object carrier.");
    }

    public static List<string> keys(object? value)
    {
        return new List<string>(Enumerate(value).Select(pair => pair.Key));
    }

    public static List<object?> values(object? value)
    {
        return new List<object?>(Enumerate(value).Select(pair => pair.Value));
    }

    public static List<(string key, object? value)> entries(object? value)
    {
        return new List<(string key, object? value)>(Enumerate(value).Select(pair => (pair.Key, pair.Value)));
    }
}
