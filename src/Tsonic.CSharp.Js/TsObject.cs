using System.Collections.Generic;
using System.Linq;

namespace Tsonic.CSharp.Js
{
    public sealed class TsObject
    {
        private readonly Dictionary<string, TsValue> _properties = new();

        public TsObject()
        {
        }

        public TsObject(IReadOnlyDictionary<string, object?> source)
        {
            foreach (var pair in source)
            {
                _properties[pair.Key] = TsValue.from(pair.Value);
            }
        }

        public TsValue ReadCompatSlot(string key)
        {
            return _properties.TryGetValue(key, out var value) ? value : TsValue.undefined();
        }

        public TsValue WriteCompatSlot(string key, object? value)
        {
            var stored = TsValue.from(value);
            _properties[key] = stored;
            return stored;
        }

        public TsValue ReadCompatElement(object? key)
        {
            return ReadCompatSlot(TsValue.propertyKey(key));
        }

        public TsValue WriteCompatElement(object? key, object? value)
        {
            return WriteCompatSlot(TsValue.propertyKey(key), value);
        }

        public IReadOnlyList<KeyValuePair<string, object?>> entries()
        {
            return _properties
                .Select(pair => new KeyValuePair<string, object?>(pair.Key, pair.Value.unwrap()))
                .ToArray();
        }
    }
}
