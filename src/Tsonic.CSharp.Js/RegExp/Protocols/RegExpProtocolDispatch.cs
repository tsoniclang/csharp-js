using System;
using System.Runtime.CompilerServices;

namespace Tsonic.CSharp.Js;

public static class RegExpProtocolDispatch
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TResult Invoke<TProtocol, TResult>(
        string input,
        TProtocol protocol,
        Func<TProtocol, string, TResult> operation)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(protocol);
        ArgumentNullException.ThrowIfNull(operation);
        return operation(protocol, input);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TResult Invoke<TProtocol, TArgument, TResult>(
        string input,
        TProtocol protocol,
        TArgument argument,
        Func<TProtocol, string, TArgument, TResult> operation)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(protocol);
        ArgumentNullException.ThrowIfNull(operation);
        return operation(protocol, input, argument);
    }
}
