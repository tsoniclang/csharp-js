using System;
using System.Collections.Generic;
using System.Linq;

namespace Tsonic.CSharp.Js
{
    public sealed class TsFunction
    {
        private readonly Func<TsValue, IReadOnlyList<TsValue>, TsValue> _call;
        private readonly Func<IReadOnlyList<TsValue>, TsValue>? _construct;
        private readonly TsObject _properties = new();

        public TsFunction(Func<IReadOnlyList<TsValue>, TsValue> call, Func<IReadOnlyList<TsValue>, TsValue>? construct = null)
            : this((_, arguments) => call(arguments), construct)
        {
        }

        public TsFunction(Func<TsValue, IReadOnlyList<TsValue>, TsValue> call, Func<IReadOnlyList<TsValue>, TsValue>? construct = null)
        {
            _call = call;
            _construct = construct;
        }

        public TsValue InvokeCompat(params object?[] arguments)
        {
            return InvokeCompatWithThis(TsValue.undefined(), arguments);
        }

        public TsValue InvokeCompatWithThis(TsValue receiver, params object?[] arguments)
        {
            return _call(receiver, arguments.Select(TsValue.from).ToArray());
        }

        public TsValue ConstructCompat(params object?[] arguments)
        {
            return _construct is null
                ? throw new TypeError("Function value is not a constructor.")
                : _construct(arguments.Select(TsValue.from).ToArray());
        }

        public TsValue ReadCompatSlot(string key)
        {
            return _properties.ReadCompatSlot(key);
        }

        public TsValue WriteCompatSlot(string key, object? value)
        {
            return _properties.WriteCompatSlot(key, value);
        }
    }
}
