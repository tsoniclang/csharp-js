using System;

namespace Tsonic.CSharp.Js;

public sealed class ReplacementCallbackArguments
{
    private readonly TsValue[] _values;

    private ReplacementCallbackArguments(TsValue[] values)
    {
        _values = (TsValue[])values.Clone();
    }

    public int length => _values.Length;

    public T Get<T>(int index)
    {
        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
        var value = index < _values.Length
            ? _values[index]
            : TsValue.undefined();
        return Project<T>(value);
    }

    public JSArray<T> Rest<T>(int index)
    {
        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
        var result = new JSArray<T>();
        for (var valueIndex = index; valueIndex < _values.Length; valueIndex += 1)
        {
            result.push(Project<T>(_values[valueIndex]));
        }
        return result;
    }

    private static T Project<T>(TsValue value) =>
        typeof(T) == typeof(TsValue)
            ? (T)(object)value
            : TsValue.CastDynamic<T>(value);

    internal static ReplacementCallbackArguments FromStringMatch(
        string match,
        int offset,
        string input) =>
        new([
            TsValue.fromnative(match),
            TsValue.fromnative(offset),
            TsValue.fromnative(input),
        ]);

    internal static ReplacementCallbackArguments FromRegExpMatch(
        RegExpExecArray match,
        string input)
    {
        var hasGroups = match.groups is not null;
        var values = new TsValue[
            match.length + 2 + (hasGroups ? 1 : 0)
        ];
        values[0] = TsValue.fromnative(match.value);
        for (var index = 1; index < match.length; index += 1)
        {
            var capture = match[index];
            values[index] = capture is null
                ? TsValue.undefined()
                : TsValue.fromnative(capture);
        }
        values[match.length] = TsValue.fromnative(match.index);
        values[match.length + 1] = TsValue.fromnative(input);
        if (hasGroups)
        {
            values[match.length + 2] = TsValue.fromnative(match.groups);
        }
        return new ReplacementCallbackArguments(values);
    }
}

public delegate string ReplacementCallback(
    ReplacementCallbackArguments arguments);
