using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Js.CostProbe;

internal sealed class NativeControl
{
    [ThreadStatic] internal static NativeControl? Current;
    private readonly int _thread = Environment.CurrentManagedThreadId;
    private readonly object _gate = new();
    private readonly Queue<Job> _ready = new();
    private readonly Queue<Job> _foreign = new();
    private int _foreignCount;
    private readonly List<Waiter?> _waiting = new();
    private readonly Stack<int> _released = new();
    private Epoch? _active;
    private long _epoch;
    internal bool Running => _active is not null;
    internal void Post(Action callback) => Post(new(callback, null, null), _epoch);

    internal void Run(Action entry)
    {
        var previous = SynchronizationContext.Current;
        var previousLoop = Current;
        var context = new Epoch(this, checked(++_epoch));
        _active = context;
        Current = this;
        SynchronizationContext.SetSynchronizationContext(context);
        try
        {
            entry();
            while (true)
            {
                if (Volatile.Read(ref _foreignCount) != 0) lock (_gate)
                {
                    while (_foreign.TryDequeue(out var posted)) _ready.Enqueue(posted);
                    Volatile.Write(ref _foreignCount, 0);
                }
                if (_ready.TryDequeue(out var job)) { job.Invoke(); continue; }
                lock (_gate)
                {
                    if (_foreign.Count != 0) continue;
                    _active = null;
                    break;
                }
            }
        }
        finally
        {
            lock (_gate) { _active = null; _foreign.Clear(); Volatile.Write(ref _foreignCount, 0); }
            foreach (var pending in _waiting) pending?.Cancel();
            _waiting.Clear();
            _released.Clear();
            _ready.Clear();
            SynchronizationContext.SetSynchronizationContext(previous);
            Current = previousLoop;
        }
    }

    internal void Wait(Task source, Action continuation)
    {
        if (source.IsCompleted) { Post(continuation); return; }
        if (_waiting.Count - _released.Count == 16384) throw new InvalidOperationException("capacity");
        var slot = _released.TryPop(out var free) ? free : _waiting.Count;
        var waiter = new Waiter(_active!, continuation, slot);
        if (slot == _waiting.Count) _waiting.Add(waiter);
        else _waiting[slot] = waiter;
        source.GetAwaiter().UnsafeOnCompleted(waiter.Notify);
    }

    private void Deliver(Waiter waiter)
    {
        _waiting[waiter.Slot] = null;
        _released.Push(waiter.Slot);
        waiter.Deliver();
    }

    private void Post(Job job, long epoch)
    {
        if (Environment.CurrentManagedThreadId == _thread)
        {
            if (_active is null || epoch != _epoch) return;
            if (_ready.Count == 1 << 20) throw new InvalidOperationException("capacity");
            _ready.Enqueue(job);
        }
        else lock (_gate)
        {
            if (_active is null || epoch != _epoch) return;
            if (_foreign.Count == 16384) throw new InvalidOperationException("capacity");
            _foreign.Enqueue(job);
            Volatile.Write(ref _foreignCount, _foreign.Count);
        }
    }

    private readonly record struct Job(Delegate? Callback, object? State, Waiter? Pending)
    {
        internal void Invoke()
        {
            if (Pending is not null) Pending.Dispatch();
            else if (Callback is Action action) action();
            else ((SendOrPostCallback)Callback!)(State);
        }
    }

    private sealed class Epoch(NativeControl owner, long identity) : SynchronizationContext
    {
        public override void Post(SendOrPostCallback callback, object? state) => owner.Post(new(callback, state, null), identity);
        internal void Notify(Waiter waiter)
        {
            owner.Post(new(null, null, waiter), identity);
        }
        internal void Deliver(Waiter waiter) => owner.Deliver(waiter);
    }

    private sealed class Waiter(Epoch owner, Action continuation, int slot)
    {
        private Action? _continuation = continuation;
        internal int Slot => slot;
        internal void Notify() => owner.Notify(this);
        internal void Dispatch() => owner.Deliver(this);
        internal void Deliver() { var callback = _continuation; _continuation = null; callback?.Invoke(); }
        internal void Cancel() => Deliver();
    }

    internal readonly struct Deferred<T>(Task<T> source) : ICriticalNotifyCompletion
    {
        public bool IsCompleted => false;
        public Deferred<T> GetAwaiter() => this;
        public T GetResult()
        {
            if (Current?.Running != true) throw new OperationCanceledException();
            return source.GetAwaiter().GetResult();
        }
        public void OnCompleted(Action continuation) => UnsafeOnCompleted(continuation);
        public void UnsafeOnCompleted(Action continuation) => Current!.Wait(source, continuation);
    }

    internal static async Task<int> Then(Task<int> source, Func<int, int> fulfilled, Func<object?, int>? rejected = null)
    {
        var pending = new Deferred<int>(source);
        int value;
        try { value = await pending; }
        catch (Exception failure) when (Current?.Running == true)
        {
            if (rejected is null) throw;
            return rejected(failure is Tsonic.CSharp.Runtime.TsThrownValueException thrown ? thrown.value : failure);
        }
        return fulfilled(value);
    }

    internal static async Task<int> Catch(Task<int> source, Func<object?, int> rejected)
    {
        var pending = new Deferred<int>(source);
        try { return await pending; }
        catch (Exception failure) when (Current?.Running == true) { return rejected(failure is Tsonic.CSharp.Runtime.TsThrownValueException thrown ? thrown.value : failure); }
    }

    internal static async Task<int> Finally(Task<int> source, Action callback)
    {
        var pending = new Deferred<int>(source);
        try { return await pending; }
        finally { if (Current?.Running == true) callback(); }
    }
}
