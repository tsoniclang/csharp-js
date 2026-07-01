using System;
using System.Collections.Generic;

namespace Tsonic.CSharp.Js
{
    public sealed class TsArray
    {
        private readonly List<TsValue> _values = new();

        public TsArray()
        {
        }

        public TsArray(IEnumerable<object?> source)
        {
            foreach (var value in source)
            {
                _values.Add(TsValue.from(value));
            }
        }

        public int length => _values.Count;

        public TsValue ReadCompatSlot(string key)
        {
            return key == "length" ? TsValue.from(length) : ReadCompatElement(key);
        }

        public TsValue WriteCompatSlot(string key, object? value)
        {
            if (key == "length")
            {
                Resize(toArrayIndex(value));
                return TsValue.from(length);
            }
            return WriteCompatElement(key, value);
        }

        public TsValue ReadCompatElement(object? key)
        {
            var index = toArrayIndex(key);
            return index >= 0 && index < _values.Count ? _values[index] : TsValue.undefined();
        }

        public TsValue WriteCompatElement(object? key, object? value)
        {
            var index = toArrayIndex(key);
            if (index < 0)
            {
                throw new RangeError("Array index cannot be negative.");
            }
            while (_values.Count <= index)
            {
                _values.Add(TsValue.undefined());
            }
            var stored = TsValue.from(value);
            _values[index] = stored;
            return stored;
        }

        private void Resize(int length)
        {
            if (length < 0)
            {
                throw new RangeError("Invalid array length.");
            }
            if (length < _values.Count)
            {
                _values.RemoveRange(length, _values.Count - length);
                return;
            }
            while (_values.Count < length)
            {
                _values.Add(TsValue.undefined());
            }
        }

        private static int toArrayIndex(object? value)
        {
            var key = TsValue.propertyKey(value);
            return int.TryParse(key, out var index) ? index : throw new TypeError("Array element access requires a numeric index.");
        }
    }
}
