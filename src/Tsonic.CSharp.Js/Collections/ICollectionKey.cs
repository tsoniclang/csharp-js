using System;

namespace Tsonic.CSharp.Js;

public interface ICollectionKey<T, TSelf> : IEquatable<TSelf>
    where TSelf : struct, ICollectionKey<T, TSelf>
{
    static abstract TSelf Create(T value);
    T Value { get; }
}
