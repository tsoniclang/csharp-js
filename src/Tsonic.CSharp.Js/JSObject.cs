using System.Collections.Generic;
using System.Linq;

namespace Tsonic.CSharp.Js;

/// <summary>
/// Closed JavaScript object carrier used by JSON and Object helpers.
/// </summary>
public sealed class JSObject
{
    private readonly Dictionary<string, object?> _properties = new();

    public object? this[string key]
    {
        get => _properties.TryGetValue(key, out var value) ? value : null;
        set => _properties[key] = value;
    }

    public bool hasOwnProperty(string key)
    {
        return _properties.ContainsKey(key);
    }

    public string[] keys()
    {
        return _properties.Keys.ToArray();
    }

    public object?[] values()
    {
        return _properties.Values.ToArray();
    }

    public (string key, object? value)[] entries()
    {
        return _properties.Select(pair => (pair.Key, pair.Value)).ToArray();
    }

    public IReadOnlyDictionary<string, object?> asReadOnlyDictionary()
    {
        return _properties;
    }
}
