using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Tsonic.CSharp.Js;

public partial class JSArray<T>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public JSArray<T> AppendElement(T value)
    {
        Add(value);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public JSArray<T> AppendSequence(IEnumerable<T> values)
    {
        AddRange(values);
        return this;
    }
}
