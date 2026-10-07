/**
 * JavaScript Timer functions implementation
 * Provides setTimeout, clearTimeout, setInterval, clearInterval
 */

using System;
using System.Collections.Concurrent;
using System.Numerics;
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

        internal sealed class TimerHandle : IDisposable
        {
            private readonly int _id;
            private readonly TimerCallback _callback;
            private readonly object?[] _arguments;
            private readonly int _period;
            private readonly Action _dispatch;
            private readonly Timer _timer;
            private int _disposed;

            internal TimerHandle(int id, TimerCallback callback, object?[] arguments, int period)
            {
                _id = id;
                _callback = callback;
                _arguments = arguments;
                _period = period;
                _dispatch = Dispatch;
                _timer = new Timer(static state => ((TimerHandle)state!).QueueCallback(), this,
                    Timeout.Infinite, Timeout.Infinite);
                ProcessKeepAlive.Acquire();
            }

            internal bool IsDisposed => Volatile.Read(ref _disposed) != 0;

            internal void Start(int delay)
            {
                lock (_timer)
                {
                    if (!IsDisposed)
                    {
                        _timer.Change(delay, _period);
                    }
                }
            }

            internal void QueueCallback()
            {
                if (!IsDisposed)
                {
                    JsEventLoop.EnqueueHandleOwned(_dispatch);
                }
            }

            private void Dispatch()
            {
                if (IsDisposed)
                {
                    return;
                }

                try
                {
                    _callback(new TimerCallbackArguments(_arguments));
                }
                finally
                {
                    if (_period == Timeout.Infinite)
                    {
                        Dispose();
                    }
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

                ProcessKeepAlive.Release();
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

        private static int NormalizeDelay(double delayMs)
        {
            if (double.IsNaN(delayMs) || double.IsInfinity(delayMs) || delayMs < 0)
            {
                return 0;
            }

            if (delayMs > int.MaxValue)
            {
                return int.MaxValue;
            }

            return (int)System.Math.Truncate(delayMs);
        }

        private static bool TryNormalizeTimerId<T>(T id, out int timerId) where T : INumberBase<T>
        {
            if (!T.IsInteger(id))
            {
                timerId = 0;
                return false;
            }

            try
            {
                timerId = int.CreateChecked(id);
                return true;
            }
            catch (OverflowException)
            {
                timerId = 0;
                return false;
            }
        }

        /// <summary>
        /// Schedule a callback to run after a delay (one-shot timer)
        /// </summary>
        public static int setTimeout(
            TimerCallback callback,
            double delayMs = 0,
            params object?[] arguments)
        {
            return Schedule(callback, NormalizeDelay(delayMs), arguments, Timeout.Infinite);
        }

        /// <summary>
        /// Cancel a timeout
        /// </summary>
        public static void clearTimeout<T>(T id) where T : INumberBase<T>
        {
            if (!TryNormalizeTimerId(id, out var timerId))
            {
                return;
            }

            if (_timers.TryGetValue(timerId, out var timer))
            {
                timer.Dispose();
            }
        }

        /// <summary>
        /// Schedule a callback to run repeatedly at an interval
        /// </summary>
        public static int setInterval(
            TimerCallback callback,
            double intervalMs = 0,
            params object?[] arguments)
        {
            var normalizedInterval = System.Math.Max(1, NormalizeDelay(intervalMs));
            return Schedule(callback, normalizedInterval, arguments, normalizedInterval);
        }

        /// <summary>
        /// Cancel an interval
        /// </summary>
        public static void clearInterval<T>(T id) where T : INumberBase<T>
        {
            // Same implementation as clearTimeout
            clearTimeout(id);
        }
    }
}
