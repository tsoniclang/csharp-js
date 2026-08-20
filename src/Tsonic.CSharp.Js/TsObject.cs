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

        public TsValue ReadDynamicSlot(string key)
        {
            return _properties.TryGetValue(key, out var value) ? value : TsValue.undefined();
        }

        public TsValue WriteDynamicSlot(string key, object? value)
        {
            var stored = TsValue.from(value);
            _properties[key] = stored;
            return stored;
        }

        public TsValue ReadDynamicElement(object? key)
        {
            return ReadDynamicSlot(TsValue.propertyKey(key));
        }

        public TsValue WriteDynamicElement(object? key, object? value)
        {
            return WriteDynamicSlot(TsValue.propertyKey(key), value);
        }

        public IReadOnlyList<KeyValuePair<string, object?>> entries()
        {
            return _properties
                .Select(pair => new KeyValuePair<string, object?>(pair.Key, pair.Value.unwrap()))
                .ToArray();
        }
    }
}
