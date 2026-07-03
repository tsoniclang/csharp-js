using System;
using System.Collections.Generic;

namespace Tsonic.CSharp.Js;

public sealed class RegExpMatchResult
{
    private readonly string?[] _groups;

    internal RegExpMatchResult(string value, int index, string input, string?[] groups)
    {
        this.value = value;
        this.index = index;
        this.input = input;
        _groups = groups;
    }

    public string value { get; }

    public int index { get; }

    public string input { get; }

    public int length => _groups.Length;

    public string? this[int groupIndex] =>
        groupIndex < 0 || groupIndex >= _groups.Length ? null : _groups[groupIndex];

    public IReadOnlyList<string?> groups => System.Array.AsReadOnly(_groups);

    public string?[] ToArray()
    {
        var copy = new string?[_groups.Length];
        System.Array.Copy(_groups, copy, _groups.Length);
        return copy;
    }
}
