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
        return TsValue.CastDynamic<T>(value);
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
            result.push(TsValue.CastDynamic<T>(_values[valueIndex]));
        }
        return result;
    }

    internal static ReplacementCallbackArguments FromStringMatch(
        string match,
        double offset,
        string input) =>
        new([
            TsValue.from(match),
            TsValue.from(offset),
            TsValue.from(input),
        ]);

    internal static ReplacementCallbackArguments FromRegExpMatch(
        RegExpExecArray match,
        string input)
    {
        var hasGroups = match.groups is not null;
        var values = new TsValue[
            match.length + 2 + (hasGroups ? 1 : 0)
        ];
        values[0] = TsValue.from(match.value);
        for (var index = 1; index < match.length; index += 1)
        {
            var capture = match[index];
            values[index] = capture is null
                ? TsValue.undefined()
                : TsValue.from(capture);
        }
        values[match.length] = TsValue.from(match.index);
        values[match.length + 1] = TsValue.from(input);
        if (hasGroups)
        {
            values[match.length + 2] = TsValue.from(match.groups);
        }
        return new ReplacementCallbackArguments(values);
    }
}

public delegate string ReplacementCallback(
    ReplacementCallbackArguments arguments);
