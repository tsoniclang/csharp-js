using System;

namespace Tsonic.CSharp.Js;

public sealed class TimerCallbackArguments
{
    private readonly TsValue[] _values;

    internal TimerCallbackArguments(object?[] values)
    {
        _values = new TsValue[values.Length];
        for (var index = 0; index < values.Length; index += 1)
        {
            _values[index] = TsValue.from(values[index]);
        }
    }

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
}

public delegate void TimerCallback(TimerCallbackArguments arguments);
