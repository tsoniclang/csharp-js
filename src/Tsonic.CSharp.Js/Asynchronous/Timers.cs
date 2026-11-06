/**
 * JavaScript Timer functions implementation
 * Provides setTimeout, clearTimeout, setInterval, clearInterval
 */

using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Tsonic.CSharp.Js
{
    /// <summary>
    /// JavaScript-style timer functions for NativeAOT
    /// </summary>
    public static class Timers
    {
        private static int _nextId = 1;
        private static readonly ConcurrentDictionary<int, TimerHandle> _timers = new();
        private static readonly DispatchPhase[] PhaseOrder = [JsEventLoop.TimerPhase];

        internal sealed class TimerHandle : IDisposable
        {
            private readonly int _id;
            private readonly TimerCallback _callback;
            private readonly object?[] _arguments;
            private readonly int _period;
            private static readonly SendOrPostCallback DispatchCallback = static state => ((TimerHandle)state!).Dispatch();
            private readonly Timer _timer;
            private readonly LoopDispatch _owner;
            private readonly LoopRegistration _registration;
            private int _disposed;
            private int _queued;

            internal TimerHandle(int id, TimerCallback callback, object?[] arguments, int period)
            {
                _id = id;
                _callback = callback;
                _arguments = arguments;
                _period = period;
                _owner = JsEventLoop.RegisterPhase(JsEventLoop.TimerPhase, PhaseOrder);
                _registration = _owner.Register(Dispose, true);
                try
                {
                    _timer = new Timer(static state => ((TimerHandle)state!).QueueCallback(), this,
                        Timeout.Infinite, Timeout.Infinite);
                }
                catch
                {
                    _registration.Dispose();
                    throw;
                }
            }

            internal bool IsDisposed => Volatile.Read(ref _disposed) != 0;

            internal void Start(int delay)
            {
                lock (_timer)
                {
                    if (!IsDisposed)
                    {
                        _timer.Change(delay, Timeout.Infinite);
                    }
                }
            }

            internal void QueueCallback()
            {
                if (!IsDisposed && Interlocked.CompareExchange(ref _queued, 1, 0) == 0)
                {
                    if (_owner.TryEnqueue(DispatchCallback, this)) return;
                    Volatile.Write(ref _queued, 0);
                    Dispose();
                    _owner.ReportFailure(new InvalidOperationException(
                        "Pending JavaScript callback count exceeds the finite event-loop limit."));
                }
            }

            private void Dispatch()
            {
                if (IsDisposed)
                {
                    Volatile.Write(ref _queued, 0);
                    return;
                }

                try
                {
                    _callback(new TimerCallbackArguments(_arguments));
                }
                finally
                {
                    Volatile.Write(ref _queued, 0);
                    if (_period == Timeout.Infinite)
                    {
                        Dispose();
                    }
                    else Start(_period);
                }
            }

            public void Dispose()
            {
                lock (_timer)
                {
                    if (IsDisposed)
                    {
                        return;
                    }

                    Volatile.Write(ref _disposed, 1);
                    _timer.Dispose();
                    _timers.TryRemove(_id, out TimerHandle? _);
                }

                _registration.Dispose();
            }
        }

        private static int Schedule(TimerCallback callback, int delay, object?[] arguments, int period)
        {
            var id = Interlocked.Increment(ref _nextId);
            var handle = new TimerHandle(id, callback, arguments, period);
            try
            {
                _timers[id] = handle;
                handle.Start(delay);
                return id;
            }
            catch
            {
                handle.Dispose();
                throw;
            }
        }


        /// <summary>
        /// Schedule a callback to run after a delay (one-shot timer)
        /// </summary>
        public static int setTimeout(
            TimerCallback callback,
            int delayMs = 0,
            params object?[] arguments)
        {
            return Schedule(callback, System.Math.Max(0, delayMs), arguments, Timeout.Infinite);
        }

        /// <summary>
        /// Cancel a timeout
        /// </summary>
        public static void clearTimeout(int? id = null)
        {
            if (!id.HasValue)
            {
                return;
            }

            if (_timers.TryGetValue(id.Value, out var timer))
            {
                timer.Dispose();
            }
        }

        /// <summary>
        /// Schedule a callback to run repeatedly at an interval
        /// </summary>
        public static int setInterval(
            TimerCallback callback,
            int intervalMs = 0,
            params object?[] arguments)
        {
            var normalizedInterval = System.Math.Max(1, intervalMs);
            return Schedule(callback, normalizedInterval, arguments, normalizedInterval);
        }

        /// <summary>
        /// Cancel an interval
        /// </summary>
        public static void clearInterval(int? id = null)
        {
            // Same implementation as clearTimeout
            clearTimeout(id);
        }
    }
}
