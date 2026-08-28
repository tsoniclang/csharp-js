using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Tsonic.CSharp.Js
{
    public static class JsEventLoop
    {
        private const int MaximumPendingCallbacks = 1 << 20;
        private static readonly BlockingCollection<PendingCallback> Pending = new(MaximumPendingCallbacks);
        private static int _running;

        public static void EnqueueReferenced(Action callback)
        {
            ArgumentNullException.ThrowIfNull(callback);
            ProcessKeepAlive.Acquire();
            if (!Pending.TryAdd(new PendingCallback(callback, true)))
            {
                ProcessKeepAlive.Release();
                throw new InvalidOperationException("Pending JavaScript callback count exceeds the finite event-loop limit.");
            }
        }

        public static void EnqueueHandleOwned(Action callback)
        {
            ArgumentNullException.ThrowIfNull(callback);
            if (!Pending.TryAdd(new PendingCallback(callback, false)))
            {
                throw new InvalidOperationException("Pending JavaScript callback count exceeds the finite event-loop limit.");
            }
        }

        public static void Run()
        {
            if (Interlocked.Exchange(ref _running, 1) != 0)
            {
                throw new InvalidOperationException("The JavaScript event loop is already running.");
            }

            try
            {
                while (true)
                {
                    if (!ProcessKeepAlive.HasReferences)
                    {
                        return;
                    }

                    if (!Pending.TryTake(out var pending, 100))
                    {
                        continue;
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
