using System;

namespace Tsonic.CSharp.Js;

public interface ICollectionKey<TValue, TSelf> : IEquatable<TSelf>
    where TSelf : struct, ICollectionKey<TValue, TSelf>
{
    static abstract TSelf Create(TValue value);
    TValue Value { get; }
    bool IsNaN { get; }
}
