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
            if (!TryEnqueue(new PendingCallback(callback, true)))
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
            return TryEnqueue(new PendingCallback(callback, false));
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
            RunCore(null);
        }

        public static void Run(Task entrypoint)
        {
            ArgumentNullException.ThrowIfNull(entrypoint);
            RunCore(entrypoint);
            entrypoint.GetAwaiter().GetResult();
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

        private static void RunCore(Task? entrypoint)
        {
            if (Interlocked.Exchange(ref _running, 1) != 0)
            {
                throw new InvalidOperationException("The JavaScript event loop is already running.");
            }

            if (entrypoint is { IsCompleted: false })
            {
                entrypoint.GetAwaiter().OnCompleted(NotifyStateChanged);
            }

            try
            {
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
                                return;
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
                        pending.Callback();
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
                Volatile.Write(ref _running, 0);
            }
        }

        private readonly record struct PendingCallback(
            Action Callback,
            bool ReleasesReference);
    }
}
