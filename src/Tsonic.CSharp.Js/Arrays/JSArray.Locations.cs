using System;
using Tsonic.CSharp.Runtime;

namespace Tsonic.CSharp.Js;

public partial class JSArray<T>
{
    public Location<T> elementLocation(int index) =>
        Location<T>.CreateIndexedElement(this, index,
            static (owner, key) => owner.tryGetAt(key, out var value)
                ? value : throw new IndexOutOfRangeException("Typed array location read requires a present element"),
            static (owner, key, value) => owner[key] = value);

    public Location<T> numericPropertyLocation(double index) =>
        IsArrayIndex(index) && index < int.MaxValue
            ? elementLocation((int)index)
            : Location<T>.CreateIndexedElement(this, index,
            static (owner, key) => owner.tryGetAt(key, out var value)
                ? value : throw new IndexOutOfRangeException("Typed array location read requires a present element"),
            static (owner, key, value) => owner[key] = value);
}
