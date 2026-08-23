using System;
using System.Collections;
using System.Collections.Generic;

namespace Tsonic.CSharp.Js;

public sealed class RegExpNamedGroups
    : IReadOnlyDictionary<string, string?>, IDynamicObject
{
    private readonly Dictionary<string, string?> _values;

    internal RegExpNamedGroups(
        IReadOnlyDictionary<string, string?> values)
    {
        _values = new Dictionary<string, string?>(values, StringComparer.Ordinal);
    }

    public string? this[string key]
    {
        get => _values.TryGetValue(key, out var value) ? value : null;
        set => _values[key] = value;
    }
    public IEnumerable<string> Keys => _values.Keys;
    public IEnumerable<string?> Values => _values.Values;
    public int Count => _values.Count;
    public bool ContainsKey(string key) => _values.ContainsKey(key);
    public bool TryGetValue(string key, out string? value) =>
        _values.TryGetValue(key, out value);
    public IEnumerator<KeyValuePair<string, string?>> GetEnumerator() =>
        _values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    bool IDynamicObject.TryReadDynamicSlot(string key, out object? value)
    {
        var found = _values.TryGetValue(key, out var group);
        value = group is null
            ? found ? Undefined.value : null
            : group;
        return found;
    }

    void IDynamicObject.WriteDynamicSlot(string key, object? value)
    {
        _values[key] = value is null or Undefined
            ? null
            : TsValue.CastDynamic<string>(value);
    }
}

public sealed class RegExpNamedIndices
    : IReadOnlyDictionary<string, (double Start, double End)?>
{
    private readonly Dictionary<string, (double Start, double End)?> _values;

    internal RegExpNamedIndices(
        IReadOnlyDictionary<
            string,
            (double Start, double End)?
        > values)
    {
        _values = new Dictionary<string, (double Start, double End)?>(
            values,
            StringComparer.Ordinal);
    }

    public (double Start, double End)? this[string key]
    {
        get => _values.TryGetValue(key, out var value) ? value : null;
        set => _values[key] = value;
    }
    public IEnumerable<string> Keys => _values.Keys;
    public IEnumerable<(double Start, double End)?> Values => _values.Values;
    public int Count => _values.Count;
    public bool ContainsKey(string key) => _values.ContainsKey(key);
    public bool TryGetValue(
        string key,
        out (double Start, double End)? value) =>
        _values.TryGetValue(key, out value);
    public IEnumerator<
        KeyValuePair<string, (double Start, double End)?>
    > GetEnumerator() => _values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public sealed class RegExpIndicesArray
    : JSArray<(double Start, double End)?>
{
    internal RegExpIndicesArray(
        (double Start, double End)?[] values,
        RegExpNamedIndices? groups)
        : base(values)
    {
        this.groups = groups;
    }

    public RegExpNamedIndices? groups { get; }
}

public class RegExpMatchArray : JSArray<string?>
{
    internal RegExpMatchArray(
        string?[] values,
        double? index,
        string? input,
        RegExpNamedGroups? groups,
        RegExpIndicesArray? indices)
        : base(values)
    {
        this.index = index;
        this.input = input;
        this.groups = groups;
        this.indices = indices;
    }

    public string value =>
        length == 0 ? string.Empty : this[0] ?? string.Empty;

    public double? index { get; }
    public string? input { get; }
    public RegExpNamedGroups? groups { get; }
    public RegExpIndicesArray? indices { get; }
}

public sealed class RegExpExecArray : RegExpMatchArray
{
    internal RegExpExecArray(
        string?[] values,
        double index,
        string input,
        RegExpNamedGroups? groups,
        RegExpIndicesArray? indices)
        : base(values, index, input, groups, indices)
    {
        this.index = index;
        this.input = input;
    }

    public new double index { get; }
    public new string input { get; }
}
