using System.Runtime.CompilerServices;

namespace Tsonic.CSharp.Js;

/// <summary>Shallow freeze state for statically closed generated reference objects.</summary>
public static class FrozenObject
{
    private sealed class State;
    private static readonly ConditionalWeakTable<object, State> States = new();

    /// <summary>Freezes an object without changing its reference identity.</summary>
    public static T Freeze<T>(T value) where T : class
    {
        States.GetValue(value, static _ => new State());
        return value;
    }

    /// <summary>Reports whether this exact reference has been frozen.</summary>
    public static bool IsFrozen<T>(T value) where T : class => States.TryGetValue(value, out _);

    /// <summary>Rejects a data-property write while leaving accessor calls unchanged.</summary>
    public static void CheckWrite(object value)
    {
        if (States.TryGetValue(value, out _))
            throw new TypeError("Cannot assign to a frozen object's data property.");
    }
}
