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

        private sealed class TimerHandle : IDisposable
        {
            private int _disposed;
            private Timer? _timer;

            public TimerHandle()
            {
                ProcessKeepAlive.Acquire();
            }

            public bool IsDisposed => Volatile.Read(ref _disposed) != 0;

            public void SetTimer(Timer timer)
            {
                _timer = timer;
            }

            public void Dispose()
            {
                if (Interlocked.Exchange(ref _disposed, 1) != 0)
                {
                    return;
                }

                var timer = Interlocked.Exchange(ref _timer, null);
                timer?.Dispose();
                ProcessKeepAlive.Release();
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

        private static bool TryNormalizeTimerId(double id, out int timerId)
        {
            if (double.IsNaN(id) || double.IsInfinity(id))
            {
                timerId = 0;
                return false;
            }

            var truncated = System.Math.Truncate(id);
            if (truncated != id || truncated < int.MinValue || truncated > int.MaxValue)
            {
                timerId = 0;
                return false;
            }

            timerId = (int)truncated;
            return true;
        }

        /// <summary>
        /// Schedule a callback to run after a delay (one-shot timer)
        /// </summary>
        public static double setTimeout(
            TimerCallback callback,
            double delayMs = 0,
            params object?[] arguments)
        {
            var id = Interlocked.Increment(ref _nextId);
            var handle = new TimerHandle();

            var timer = new Timer(_ =>
            {
                if (handle.IsDisposed)
                {
                    return;
                }

                JsEventLoop.EnqueueReferenced(() =>
                {
                    try
                    {
                        callback(new TimerCallbackArguments(arguments));
                    }
                    finally
                    {
                        _timers.TryRemove(id, out TimerHandle? _);
                        handle.Dispose();
                    }
                });
            }, null, NormalizeDelay(delayMs), Timeout.Infinite);

            handle.SetTimer(timer);
            _timers[id] = handle;
            return id;
        }

        /// <summary>
        /// Cancel a timeout
        /// </summary>
        public static void clearTimeout(double id)
        {
            if (!TryNormalizeTimerId(id, out var timerId))
            {
                return;
            }

            if (_timers.TryRemove(timerId, out var timer))
            {
                timer.Dispose();
            }
        }

        /// <summary>
        /// Schedule a callback to run repeatedly at an interval
        /// </summary>
        public static double setInterval(
            TimerCallback callback,
            double intervalMs = 0,
            params object?[] arguments)
        {
            var id = Interlocked.Increment(ref _nextId);
            var handle = new TimerHandle();
            var normalizedInterval = NormalizeDelay(intervalMs);

            var timer = new Timer(_ =>
            {
                if (!handle.IsDisposed)
                {
                    JsEventLoop.EnqueueReferenced(() =>
                    {
                        if (!handle.IsDisposed)
                        {
                            callback(new TimerCallbackArguments(arguments));
                        }
                    });
                }
            }, null, normalizedInterval, normalizedInterval);

            handle.SetTimer(timer);
            _timers[id] = handle;
            return id;
        }

        /// <summary>
        /// Cancel an interval
        /// </summary>
        public static void clearInterval(double id)
        {
            // Same implementation as clearTimeout
            clearTimeout(id);
        }
    }
}
