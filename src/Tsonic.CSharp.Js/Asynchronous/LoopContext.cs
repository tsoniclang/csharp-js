using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Js;

internal sealed class LoopContext
{
    private readonly int _threadId = Environment.CurrentManagedThreadId;
    private readonly object _sync = new();
    private readonly Queue<(int Phase, PendingCallback Callback)> _ingress = new();
    private readonly List<ReadyPhase> _phases = [new(default)];
    private readonly Dictionary<long, OwnedRegistration> _registrations = new();
    private readonly List<NativeTaskWaiter?> _waiters = new();
    private readonly Stack<int> _freeWaiters = new();
    private LoopSynchronizationContext? _activation;
    private long _generation;
    private long _nextRegistration;
    private int _running;
    private int _ingressCount;
    private int _readyCount;
    private int _phaseCount = 1;
    private int _references;
    private int _maximumCallbacks;
    private int _maximumIngress;
    private int _maximumRegistrations;
    private bool _exitRequested;
    private ExceptionDispatchInfo? _failure;

    internal bool IsRunning => Volatile.Read(ref _running) != 0;
    internal bool HasReferences => Volatile.Read(ref _references) != 0;
    internal bool IsActive(long generation) => IsRunning && Volatile.Read(ref _generation) == generation;
    internal LoopDispatch Capture(int phase) => IsRunning ? new(this, _generation, phase)
        : throw new InvalidOperationException(JsEventLoop.MissingLoopMessage);

    internal LoopDispatch RegisterPhase(DispatchPhase phase, ReadOnlySpan<DispatchPhase> order)
    {
        if (!phase.IsValid) throw new ArgumentException("A dispatch phase requires its registration identity.", nameof(phase));
        if (order.Length > 64) throw new ArgumentException("Ordered dispatch phase count exceeds its finite limit.", nameof(order));
        var selected = -1;
        for (var index = 0; index < order.Length; index++)
        {
            if (!order[index].IsValid) throw new ArgumentException("An ordered dispatch phase is unregistered.", nameof(order));
            if (index + 1 < _phaseCount && !_phases[index + 1].Identity.Equals(order[index]))
                throw new InvalidOperationException("The event loop already has a different ordered phase registration.");
            for (var previous = 0; previous < index; previous++)
                if (order[index].Equals(order[previous])) throw new ArgumentException("Ordered dispatch phases must have distinct identities.", nameof(order));
            if (order[index].Equals(phase)) selected = index + 1;
        }
        if (selected < 0) throw new ArgumentException("The dispatch phase is absent from its ordered registration.", nameof(phase));
        for (var index = _phaseCount - 1; index < order.Length; index++)
        {
            if (index + 1 < _phases.Count) _phases[index + 1].Identity = order[index];
            else _phases.Add(new(order[index]));
        }
        _phaseCount = System.Math.Max(_phaseCount, order.Length + 1);
        return Capture(selected);
    }

    internal bool TryEnqueue(long generation, int phase, PendingCallback callback)
    {
        if (!IsActive(generation)) return false;
        if (Environment.CurrentManagedThreadId == _threadId)
        {
            if (_readyCount == _maximumCallbacks) return false;
            _phases[phase].Pending.Enqueue(callback);
            _readyCount++;
            return true;
        }
        lock (_sync)
        {
            if (!IsActive(generation) || _ingress.Count == _maximumIngress) return false;
            _ingress.Enqueue((phase, callback));
            Volatile.Write(ref _ingressCount, _ingress.Count);
            Monitor.Pulse(_sync);
            return true;
        }
    }

    internal LoopRegistration Register(long generation, PendingCallback shutdown, bool referenced)
    {
        lock (_sync)
        {
            if (!IsActive(generation)) throw new InvalidOperationException(JsEventLoop.MissingLoopMessage);
            RequireRegistrationCapacity();
            var identity = checked(++_nextRegistration);
            _registrations.Add(identity, new(shutdown, referenced));
            if (referenced) ChangeReferences(generation, 1);
            return new(this, generation, identity);
        }
    }

    internal void Unregister(long generation, long identity)
    {
        lock (_sync)
        {
            if (!IsActive(generation)) return;
            if (_registrations.Remove(identity, out var registration) && registration.Referenced) ChangeReferences(generation, -1);
        }
    }

    internal void ChangeReferences(long generation, int delta)
    {
        if (Environment.CurrentManagedThreadId != _threadId)
        {
            lock (_sync) ChangeReferencesCore(generation, delta);
        }
        else ChangeReferencesCore(generation, delta);
    }

    private void ChangeReferencesCore(long generation, int delta)
    {
        if (!IsActive(generation))
        {
            if (delta < 0) return;
            throw new InvalidOperationException(JsEventLoop.MissingLoopMessage);
        }
        while (true)
        {
            var current = Volatile.Read(ref _references);
            var next = checked(current + delta);
            if (next < 0) throw new InvalidOperationException("Native event-loop references cannot underflow.");
            if (Interlocked.CompareExchange(ref _references, next, current) != current) continue;
            if (next == 0 && Environment.CurrentManagedThreadId != _threadId) NotifyStateChanged(generation);
            return;
        }
    }

    internal void ReportFailure(Exception failure, long generation)
    {
        ArgumentNullException.ThrowIfNull(failure);
        if (Environment.CurrentManagedThreadId == _threadId)
        {
            if (IsActive(generation)) Interlocked.CompareExchange(ref _failure, ExceptionDispatchInfo.Capture(failure), null);
            return;
        }
        lock (_sync)
        {
            if (!IsActive(generation)) return;
            Interlocked.CompareExchange(ref _failure, ExceptionDispatchInfo.Capture(failure), null);
            Monitor.Pulse(_sync);
        }
    }

    internal void RequestExit(long generation) { RequireOwner(generation); _exitRequested = true; }
    internal void NotifyStateChanged(long generation)
    {
        lock (_sync) { if (IsActive(generation)) Monitor.Pulse(_sync); }
    }

    internal void Defer(Task source, Action continuation)
    {
        if (source.IsCompleted)
        {
            Capture(0).Enqueue(continuation);
            return;
        }
        Observe(source, continuation);
    }

    internal void Observe(Task source, Action? continuation)
    {
        NativeTaskWaiter waiter;
        lock (_sync)
        {
            RequireRegistrationCapacity();
            var identity = _freeWaiters.TryPop(out var free) ? free : _waiters.Count;
            waiter = continuation is null
                ? new DiscardedTaskWaiter(_activation!, identity, source)
                : new ReactionTaskWaiter(_activation!, identity, continuation);
            if (identity == _waiters.Count) _waiters.Add(waiter);
            else _waiters[identity] = waiter;
        }
        source.GetAwaiter().UnsafeOnCompleted(waiter.Complete);
    }

    private void RequireRegistrationCapacity()
    {
        if (_registrations.Count + _waiters.Count - _freeWaiters.Count == _maximumRegistrations)
            throw new InvalidOperationException("JavaScript event-loop registration count exceeds its finite limit.");
    }

    internal void CompleteWaiter(long generation, NativeTaskWaiter waiter)
    {
        if (!TryEnqueue(generation, 0, new PendingCallback(NativeTaskWaiter.InvokeCallback, waiter)) && IsActive(generation))
            ReportFailure(new InvalidOperationException(JsEventLoop.CapacityMessage), generation);
    }

    internal void InvokeWaiter(long generation, NativeTaskWaiter waiter)
    {
        RequireOwner(generation);
        lock (_sync)
        {
            _waiters[waiter.Identity] = null;
            _freeWaiters.Push(waiter.Identity);
        }
        waiter.Invoke();
    }

    internal void Run(Action? initialize, Func<Task>? start, int maximumCallbacks, int maximumIngress, int maximumRegistrations)
    {
        if (maximumCallbacks <= 0 || maximumIngress <= 0 || maximumRegistrations <= 0) throw new ArgumentOutOfRangeException(nameof(maximumCallbacks), "Event-loop limits must be positive finite integers.");
        if (IsRunning) throw new InvalidOperationException("The JavaScript event loop is already running on this thread.");
        var previous = SynchronizationContext.Current;
        ExceptionDispatchInfo? failure = null;
        _maximumCallbacks = maximumCallbacks;
        _maximumIngress = maximumIngress;
        _maximumRegistrations = maximumRegistrations;
        _exitRequested = false;
        _phaseCount = 1;
        _generation = checked(_generation + 1);
        var context = new LoopSynchronizationContext(this, _generation);
        _activation = context;
        Volatile.Write(ref _running, 1);
        SynchronizationContext.SetSynchronizationContext(context);
        try
        {
            initialize?.Invoke();
            var root = start?.Invoke();
            if (start is not null && root is null) throw new InvalidOperationException("The JavaScript entry factory returned no Task.");
            if (root is { IsCompleted: false }) root.GetAwaiter().UnsafeOnCompleted(context.Notify);
            Drive(root);
        }
        catch (Exception exception) { failure = ExceptionDispatchInfo.Capture(exception); }
        finally
        {
            var shutdownFailure = Shutdown();
            SynchronizationContext.SetSynchronizationContext(previous);
            failure ??= shutdownFailure;
        }
        failure?.Throw();
    }

    private void Drive(Task? root)
    {
        while (!_exitRequested)
        {
            Volatile.Read(ref _failure)?.Throw();
            if (root is { IsCompleted: true, IsCompletedSuccessfully: false }) root.GetAwaiter().GetResult();
            DrainIngress();
            if (TryTake(out var callback)) { callback.Invoke(); continue; }
            lock (_sync)
            {
                if (_ingress.Count != 0 || Volatile.Read(ref _failure) is not null) continue;
                if ((root is null || root.IsCompleted) && !HasReferences)
                {
                    Volatile.Write(ref _running, 0);
                    return;
                }
                Monitor.Wait(_sync);
            }
        }
    }

    private void DrainIngress()
    {
        if (Volatile.Read(ref _ingressCount) == 0) return;
        lock (_sync)
        {
            while (_readyCount < _maximumCallbacks && _ingress.TryDequeue(out var pending))
            {
                _phases[pending.Phase].Pending.Enqueue(pending.Callback);
                _readyCount++;
            }
            Volatile.Write(ref _ingressCount, _ingress.Count);
        }
    }

    private bool TryTake(out PendingCallback callback)
    {
        if (_readyCount != 0) for (var index = 0; index < _phaseCount; index++)
            if (_phases[index].Pending.TryDequeue(out callback)) { _readyCount--; return true; }
        callback = default;
        return false;
    }

    private ExceptionDispatchInfo? Shutdown()
    {
        lock (_sync)
        {
            Volatile.Write(ref _running, 0);
            _ingress.Clear();
            Volatile.Write(ref _ingressCount, 0);
            Monitor.PulseAll(_sync);
        }
        foreach (var phase in _phases) phase.Pending.Clear();
        foreach (var waiter in _waiters) waiter?.Cancel();
        _waiters.Clear();
        _freeWaiters.Clear();
        _readyCount = 0;
        var failure = _failure;
        _failure = null;
        while (_registrations.Count != 0)
        {
            using var cursor = _registrations.GetEnumerator();
            cursor.MoveNext();
            var entry = cursor.Current;
            _registrations.Remove(entry.Key);
            try { entry.Value.Shutdown.Invoke(); }
            catch (Exception exception) { failure ??= ExceptionDispatchInfo.Capture(exception); }
        }
        Volatile.Write(ref _references, 0);
        _activation = null;
        return failure;
    }

    private void RequireOwner(long generation)
    {
        if (!IsActive(generation)) throw new InvalidOperationException(JsEventLoop.MissingLoopMessage);
        if (Environment.CurrentManagedThreadId != _threadId) throw new InvalidOperationException("A JavaScript loop registration must be changed on its owner thread.");
    }

    internal void Post(long generation, SendOrPostCallback callback, object? state)
    {
        ArgumentNullException.ThrowIfNull(callback);
        if (!TryEnqueue(generation, 0, new(callback, state)) && IsActive(generation)) ReportFailure(new InvalidOperationException(JsEventLoop.CapacityMessage), generation);
    }

    internal void Send(long generation, SendOrPostCallback callback, object? state)
    {
        ArgumentNullException.ThrowIfNull(callback);
        if (!IsActive(generation)) throw new InvalidOperationException(JsEventLoop.MissingLoopMessage);
        var dispatch = new LoopDispatch(this, generation, 0);
        if (Environment.CurrentManagedThreadId == _threadId) { callback(state); return; }
        using var completed = new ManualResetEventSlim();
        ExceptionDispatchInfo? failure = null;
        var disposed = false;
        using var registration = dispatch.Register(() =>
        {
            lock (completed)
            {
                if (disposed) return;
                failure = ExceptionDispatchInfo.Capture(new OperationCanceledException("The JavaScript event-loop owner has shut down."));
                completed.Set();
            }
        }, true);
        try
        {
            dispatch.Enqueue(() =>
            {
                try { callback(state); }
                catch (Exception exception) { failure = ExceptionDispatchInfo.Capture(exception); }
                finally
                {
                    registration.Dispose();
                    lock (completed) { if (!disposed) completed.Set(); }
                }
            });
            completed.Wait();
            failure?.Throw();
        }
        finally
        {
            lock (completed) disposed = true;
        }
    }

    private sealed class ReadyPhase(DispatchPhase identity)
    {
        internal DispatchPhase Identity = identity;
        internal readonly Queue<PendingCallback> Pending = new();
    }
    private readonly record struct OwnedRegistration(PendingCallback Shutdown, bool Referenced);

}
