using System;
using System.Collections.Generic;
using System.Globalization;

namespace Tsonic.CSharp.Js;

public partial class JSArray<T>
{
    private List<(double Key, T Value)>? _numericProperties;

    public T this[double index]
    {
        get => tryGetAt(index, out var value) ? value : default!;
        set
        {
            if (IsArrayIndex(index))
            {
                if (index >= int.MaxValue) throw new RangeError("Array index exceeds native storage");
                this[(int)index] = value;
                return;
            }
            _numericProperties ??= [];
            var position = FindNumericProperty(index);
            if (position < 0) _numericProperties.Add((index, value));
            else _numericProperties[position] = (index, value);
        }
    }

    public bool tryGetAt(double index, out T value)
    {
        if (IsArrayIndex(index))
        {
            if (index < _values.Count) return tryGetAt((int)index, out value);
        }
        else
        {
            var position = FindNumericProperty(index);
            if (position >= 0)
            {
                value = _numericProperties![position].Value;
                return true;
            }
        }
        value = default!;
        return false;
    }

    public bool hasIndex(double index) => tryGetAt(index, out _);

    public bool deleteAt(double index)
    {
        if (IsArrayIndex(index))
        {
            if (index < _values.Count) return deleteAt((int)index);
        }
        else
        {
            var position = FindNumericProperty(index);
            if (position >= 0) _numericProperties!.RemoveAt(position);
        }
        return true;
    }

    public int setLength(double value) => setLength(ToArrayLength(value));

    bool IDynamicArray.HasOwn(string key) =>
        key == "length" || TryNumericKey(key, out var index) && hasIndex(index);

    IEnumerable<KeyValuePair<string, object?>> IDynamicArray.Entries()
    {
        for (var index = 0; index < _values.Count; index++)
        {
            if (tryGetAt(index, out var value))
                yield return new(index.ToString(CultureInfo.InvariantCulture), value);
        }
        if (_numericProperties is not null)
        {
            foreach (var property in _numericProperties)
                yield return new(Number.toString(property.Key), property.Value);
        }
    }

    bool IDynamicObject.TryReadDynamicSlot(string key, out object? value)
    {
        if (key == "length")
        {
            value = length;
            return true;
        }
        if (TryNumericKey(key, out var index) && tryGetAt(index, out var element))
        {
            value = element;
            return true;
        }
        value = null;
        return false;
    }

    void IDynamicArray.VisitElements<TVisitor>(ref TVisitor visitor)
    {
        for (var index = 0; index < _values.Count; index++) visitor.Visit(_values[index]);
    }

    void IDynamicObject.WriteDynamicSlot(string key, object? value)
    {
        if (key == "length")
        {
            if (value is TsValue wrapped) value = wrapped.unwrap();
            if (value is System.Numerics.BigInteger)
                throw new TypeError("Array length cannot be a BigInt");
            setLength(Globals.Number(value));
            return;
        }
        if (!TryNumericKey(key, out var index))
            throw new TypeError("Closed arrays require a numeric property key");
        if (TryAssignmentElement(value, out var typed)) this[index] = typed;
        else throw new TypeError("Value does not match the closed array element carrier");
    }

    private static bool TryAssignmentElement(object? value, out T element)
    {
        if (typeof(T) == typeof(TsValue))
        {
            var closed = TsValue.from(value);
            element = System.Runtime.CompilerServices.Unsafe.As<TsValue, T>(ref closed);
            return true;
        }
        if (value is TsValue wrapped) value = wrapped.unwrap();
        if (value is T typed)
        {
            element = typed;
            return true;
        }
        if (value is null && default(T) is null)
        {
            element = default!;
            return true;
        }
        element = default!;
        return false;
    }

    private int FindNumericProperty(double key)
    {
        if (_numericProperties is null) return -1;
        for (var index = 0; index < _numericProperties.Count; index++)
        {
            if (_numericProperties[index].Key.Equals(key)) return index;
        }
        return -1;
    }

    private static bool IsArrayIndex(double index) =>
        double.IsFinite(index) && index >= 0 &&
        index == System.Math.Truncate(index);

    private static bool TryNumericKey(string key, out double index) =>
        double.TryParse(key, NumberStyles.Float, CultureInfo.InvariantCulture, out index) &&
        Number.toString(index) == key;
}
