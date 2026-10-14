using System;

namespace Tsonic.CSharp.Js;

public sealed class PhaseRegistration
{
    public DispatchPhase Phase => new(this);
}

public readonly struct DispatchPhase : IEquatable<DispatchPhase>
{
    private readonly PhaseRegistration? _identity;
    internal DispatchPhase(PhaseRegistration identity) => _identity = identity;
    internal bool IsValid => _identity is not null;
    public bool Equals(DispatchPhase other) => ReferenceEquals(_identity, other._identity);
    public override bool Equals(object? other) => other is DispatchPhase phase && Equals(phase);
    public override int GetHashCode() => _identity?.GetHashCode() ?? 0;
}

public readonly struct LoopDispatch
{
    private readonly LoopContext? _owner;
    private readonly long _generation;
    private readonly int _phase;
    internal LoopDispatch(LoopContext owner, long generation, int phase)
    {
        _owner = owner;
        _generation = generation;
        _phase = phase;
    }
    public bool IsActive => _owner?.IsActive(_generation) == true;
    public bool IsBound => _owner is not null;
    public bool TryEnqueue(Action callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        return Owner.TryEnqueue(_generation, _phase, new PendingCallback(callback, null));
    }
    public bool TryEnqueue(System.Threading.SendOrPostCallback callback, object? state)
    {
        ArgumentNullException.ThrowIfNull(callback);
        return Owner.TryEnqueue(_generation, _phase, new PendingCallback(callback, state));
    }
    public void Enqueue(System.Threading.SendOrPostCallback callback, object? state)
    {
        if (!TryEnqueue(callback, state)) throw new InvalidOperationException(IsActive ? JsEventLoop.CapacityMessage : JsEventLoop.MissingLoopMessage);
    }
    public void Enqueue(Action callback)
    {
        if (!TryEnqueue(callback)) throw new InvalidOperationException(IsActive ? JsEventLoop.CapacityMessage : JsEventLoop.MissingLoopMessage);
    }
    public LoopRegistration Register(Action shutdown, bool referenced = false)
    {
        ArgumentNullException.ThrowIfNull(shutdown);
        return Owner.Register(_generation, new(shutdown, null), referenced);
    }
    public LoopRegistration Register(System.Threading.SendOrPostCallback shutdown, object? state, bool referenced = false)
    {
        ArgumentNullException.ThrowIfNull(shutdown);
        return Owner.Register(_generation, new(shutdown, state), referenced);
    }
    public void Acquire() => Owner.ChangeReferences(_generation, 1);
    public void Release() => Owner.ChangeReferences(_generation, -1);
    public void ReportFailure(Exception failure) => Owner.ReportFailure(failure, _generation);
    internal void RequestExit() => Owner.RequestExit(_generation);
    internal LoopContext Owner => _owner ?? throw new InvalidOperationException(JsEventLoop.MissingLoopMessage);
}

public readonly struct LoopRegistration : IDisposable
{
    private readonly LoopContext? _owner;
    private readonly long _generation;
    private readonly long _identity;
    internal LoopRegistration(LoopContext owner, long generation, long identity)
    {
        _owner = owner;
        _generation = generation;
        _identity = identity;
    }
    public void Dispose() => _owner?.Unregister(_generation, _identity);
}

internal readonly record struct PendingCallback(Delegate Callback, object? State)
{
    internal void Invoke()
    {
        if (Callback is Action action) action();
        else ((System.Threading.SendOrPostCallback)Callback)(State);
    }
}
