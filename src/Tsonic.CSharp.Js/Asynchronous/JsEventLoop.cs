using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Js
{
    public static class JsEventLoop
    {
        private const int MaximumPendingCallbacks = 1 << 20;
        private static readonly object Sync = new();
        private static readonly Queue<PendingCallback> Pending = new();
        private static int _running;
        private static ExceptionDispatchInfo? _failure;

        public static void EnqueueReferenced(Action callback)
        {
            ArgumentNullException.ThrowIfNull(callback);
            ProcessKeepAlive.Acquire();
            if (!TryEnqueue(new PendingCallback(callback, null, true)))
            {
                ProcessKeepAlive.Release();
                throw new InvalidOperationException("Pending JavaScript callback count exceeds the finite event-loop limit.");
            }
        }

        public static void EnqueueHandleOwned(Action callback)
        {
            if (!TryEnqueueHandleOwned(callback))
            {
                throw new InvalidOperationException("Pending JavaScript callback count exceeds the finite event-loop limit.");
            }
        }

        internal static bool TryEnqueueHandleOwned(Action callback)
        {
            ArgumentNullException.ThrowIfNull(callback);
            return TryEnqueue(new PendingCallback(callback, null, false));
        }

        internal static void ReportFailure(Exception failure)
        {
            lock (Sync)
            {
                _failure ??= ExceptionDispatchInfo.Capture(failure);
                Monitor.PulseAll(Sync);
            }
        }

        public static void Run()
        {
            RunCore(null, null);
        }

        public static void Run(Action entrypoint)
        {
            ArgumentNullException.ThrowIfNull(entrypoint);
            RunCore(entrypoint, null);
        }

        public static void Run(Func<Task> entrypoint)
        {
            ArgumentNullException.ThrowIfNull(entrypoint);
            RunCore(null, entrypoint);
        }

        internal static void NotifyStateChanged()
        {
            lock (Sync)
            {
                Monitor.PulseAll(Sync);
            }
        }

        private static bool TryEnqueue(PendingCallback callback)
        {
            lock (Sync)
            {
                if (Pending.Count == MaximumPendingCallbacks)
                {
                    return false;
                }

                Pending.Enqueue(callback);
                Monitor.PulseAll(Sync);
                return true;
            }
        }

        private static void RunCore(Action? initialize, Func<Task>? start)
        {
            if (Interlocked.Exchange(ref _running, 1) != 0)
            {
                throw new InvalidOperationException("The JavaScript event loop is already running.");
            }

            var previousContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(new LoopContext(Environment.CurrentManagedThreadId));
            try
            {
                initialize?.Invoke();
                var entrypoint = start?.Invoke();
                if (start is not null && entrypoint is null)
                    throw new InvalidOperationException("The JavaScript entry factory returned no Task.");
                if (entrypoint is { IsCompleted: false })
                    entrypoint.GetAwaiter().OnCompleted(NotifyStateChanged);
                while (true)
                {
                    PendingCallback pending;
                    lock (Sync)
                    {
                        while (true)
                        {
                            if (_failure is { } failure)
                            {
                                _failure = null;
                                failure.Throw();
                            }
                            if (entrypoint is { IsCompleted: true, IsCompletedSuccessfully: false })
                            {
                                entrypoint.GetAwaiter().GetResult();
                            }
                            if (Pending.Count != 0)
                            {
                                pending = Pending.Dequeue();
                                break;
                            }
                            if (
                                (entrypoint is null || entrypoint.IsCompletedSuccessfully) &&
                                !ProcessKeepAlive.HasReferences
                            )
                            {
                                return;
                            }
                            Monitor.Wait(Sync);
                        }
                    }

                    try
                    {
                        pending.Invoke();
                    }
                    finally
                    {
                        if (pending.ReleasesReference)
                        {
                            ProcessKeepAlive.Release();
                        }
                    }
                }
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previousContext);
                Volatile.Write(ref _running, 0);
            }
        }

        private sealed class LoopContext(int threadId) : SynchronizationContext
        {
            public override void Post(SendOrPostCallback callback, object? state)
            {
                ArgumentNullException.ThrowIfNull(callback);
                ProcessKeepAlive.Acquire();
                if (TryEnqueue(new PendingCallback(callback, state, true))) return;
                ProcessKeepAlive.Release();
                ReportFailure(new InvalidOperationException("Pending JavaScript callback count exceeds the finite event-loop limit."));
            }

            public override void Send(SendOrPostCallback callback, object? state)
            {
                ArgumentNullException.ThrowIfNull(callback);
                if (Environment.CurrentManagedThreadId == threadId) { callback(state); return; }
                using var completed = new ManualResetEventSlim();
                ExceptionDispatchInfo? failure = null;
                EnqueueReferenced(() =>
                {
                    try { callback(state); }
                    catch (Exception exception) { failure = ExceptionDispatchInfo.Capture(exception); }
                    finally { completed.Set(); }
                });
                completed.Wait();
                failure?.Throw();
            }
        }

        private readonly record struct PendingCallback(
            Delegate Callback,
            object? State,
            bool ReleasesReference)
        {
            public void Invoke()
            {
                if (Callback is Action action) action();
                else ((SendOrPostCallback)Callback)(State);
            }
        }
    }
}
