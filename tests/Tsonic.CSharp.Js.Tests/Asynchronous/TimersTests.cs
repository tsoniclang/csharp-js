using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tsonic.CSharp.Js.Tests
{
    [CollectionDefinition("JavaScript event loop", DisableParallelization = true)]
    public sealed class JavaScriptEventLoopCollection
    {
    }

    [Collection("JavaScript event loop")]
    public class TimersTests
    {
        private static async Task WaitFor(ManualResetEventSlim signal, int timeoutMs, string message)
        {
            Assert.True(await Task.Run(() => signal.Wait(timeoutMs)), message);
        }

        private static async Task WaitUntil(Func<bool> predicate, int timeoutMs, string message)
        {
            var deadline = Stopwatch.GetTimestamp() + (long)(timeoutMs * (Stopwatch.Frequency / 1000.0));
            while (Stopwatch.GetTimestamp() < deadline)
            {
                if (predicate()) return;
                await Task.Delay(5);
            }

            Assert.True(predicate(), message);
        }

        private static async Task AssertStays(Func<bool> predicate, int durationMs, string message)
        {
            var deadline = Stopwatch.GetTimestamp() + (long)(durationMs * (Stopwatch.Frequency / 1000.0));
            while (Stopwatch.GetTimestamp() < deadline)
            {
                Assert.True(predicate(), message);
                await Task.Delay(5);
            }
        }

        // ==================== setTimeout Tests ====================

        [Fact]
        public void setTimeout_ExecutesCallbackAfterDelay()
        {
            JsEventLoop.Run(async () =>
            {
                using var executed = new ManualResetEventSlim(false);
                var id = Timers.setTimeout(_ => executed.Set(), 50);
    
                Assert.False(executed.IsSet);
                await WaitFor(executed, 1000, "setTimeout callback did not execute within the expected timeout window.");
            });
        }

        [Fact]
        public void setTimeout_ReturnsUniqueId()
        {
            JsEventLoop.Run(() =>
            {
                var id1 = Timers.setTimeout(_ => { }, 1000);
                var id2 = Timers.setTimeout(_ => { }, 1000);
    
                Assert.NotEqual(id1, id2);
    
                // Clean up
                Timers.clearTimeout(id1);
                Timers.clearTimeout(id2);
            });
        }

        [Fact]
        public void setTimeout_WithZeroDelay_ExecutesCallback()
        {
            JsEventLoop.Run(async () =>
            {
                using var executed = new ManualResetEventSlim(false);
                Timers.setTimeout(_ => executed.Set(), 0);
    
                await WaitFor(executed, 1000, "Zero-delay setTimeout did not execute.");
            });
        }

        [Fact]
        public void setTimeout_TruncatesFractionalDelay()
        {
            JsEventLoop.Run(async () =>
            {
                using var executed = new ManualResetEventSlim(false);
                Timers.setTimeout(_ => executed.Set(), 10.9);
    
                await WaitFor(executed, 1000, "Fractional-delay setTimeout did not execute.");
            });
        }

        [Fact]
        public void setTimeout_DefaultDelay_IsZero()
        {
            JsEventLoop.Run(async () =>
            {
                using var executed = new ManualResetEventSlim(false);
                Timers.setTimeout(_ => executed.Set());
    
                await WaitFor(executed, 1000, "Default-delay setTimeout did not execute.");
            });
        }

        [Fact]
        public void setTimeout_ExecutesOnlyOnce()
        {
            JsEventLoop.Run(async () =>
            {
                var count = 0;
                Timers.setTimeout(_ => count++, 50);
    
                await WaitUntil(() => Volatile.Read(ref count) == 1, 1000, "setTimeout did not execute exactly once.");
                await AssertStays(() => Volatile.Read(ref count) == 1, 250, "setTimeout executed more than once.");
                Assert.Equal(1, count);
            });
        }

        [Fact]
        public void setTimeout_WithArgument_PassesArgument()
        {
            JsEventLoop.Run(async () =>
            {
                string? received = null;
                using var delivered = new ManualResetEventSlim(false);
                Timers.setTimeout(arguments =>
                {
                    received = arguments.Get<string>(0);
                    delivered.Set();
                }, 50, "hello");
    
                await WaitFor(delivered, 1000, "setTimeout with argument did not execute.");
                Assert.Equal("hello", received);
            });
        }

        // ==================== clearTimeout Tests ====================

        [Fact]
        public void clearTimeout_PreventsExecution()
        {
            JsEventLoop.Run(async () =>
            {
                using var executed = new ManualResetEventSlim(false);
                var id = Timers.setTimeout(_ => executed.Set(), 100);
    
                Timers.clearTimeout(id);
                Assert.False(await Task.Run(() => executed.Wait(300)), "clearTimeout did not prevent callback execution.");
            });
        }

        [Fact]
        public void clearTimeout_WithInvalidId_DoesNotThrow()
        {
            JsEventLoop.Run(() =>
            {
                // Should not throw for non-existent ID
                var exception = Record.Exception(() => Timers.clearTimeout(99999));
                Assert.Null(exception);
            });
        }

        [Fact]
        public void clearTimeout_WithNonIntegralId_DoesNotThrow()
        {
            JsEventLoop.Run(() =>
            {
                var exception = Record.Exception(() => Timers.clearTimeout(99999.5));
                Assert.Null(exception);
            });
        }

        [Fact]
        public void TimerIdsRetainNativeIntegersAndRejectInvalidNumbers()
        {
            JsEventLoop.Run(() =>
            {
                int timeout = Timers.setTimeout(_ => throw new InvalidOperationException(), 100000);
                int interval = Timers.setInterval(_ => throw new InvalidOperationException(), 100000);
                Timers.clearTimeout(timeout);
                Timers.clearInterval(interval);
                Timers.clearTimeout(ulong.MaxValue);
                Timers.clearTimeout(9007199254740993L);
                Timers.clearTimeout(double.NaN);
                Timers.clearTimeout(double.PositiveInfinity);
                Timers.clearTimeout(2147483648.0);
                Timers.clearInterval(-2147483649.0);
            });
        }

        [Fact]
        public void clearTimeout_CalledTwice_DoesNotThrow()
        {
            JsEventLoop.Run(() =>
            {
                var id = Timers.setTimeout(_ => { }, 1000);
    
                Timers.clearTimeout(id);
                var exception = Record.Exception(() => Timers.clearTimeout(id));
                Assert.Null(exception);
            });
        }

        [Fact]
        public void clearTimeout_AfterExecution_DoesNotThrow()
        {
            JsEventLoop.Run(async () =>
            {
                using var executed = new ManualResetEventSlim(false);
                var id = Timers.setTimeout(_ => executed.Set(), 10);
    
                await WaitFor(executed, 1000, "Timeout did not execute before clearTimeout-after-execution check.");
                var exception = Record.Exception(() => Timers.clearTimeout(id));
                Assert.Null(exception);
            });
        }

        // ==================== setInterval Tests ====================

        [Fact]
        public void setInterval_ExecutesRepeatedly()
        {
            JsEventLoop.Run(async () =>
            {
                var count = 0;
                var id = Timers.setInterval(_ => count++, 50);
    
                await WaitUntil(() => Volatile.Read(ref count) >= 2, 1000, $"Expected at least 2 executions, got {Volatile.Read(ref count)}.");
                Timers.clearInterval(id);
    
                Assert.True(count >= 2, $"Expected at least 2 executions, got {count}");
            });
        }

        [Fact]
        public void setInterval_ReturnsUniqueId()
        {
            JsEventLoop.Run(() =>
            {
                var id1 = Timers.setInterval(_ => { }, 1000);
                var id2 = Timers.setInterval(_ => { }, 1000);
    
                Assert.NotEqual(id1, id2);
    
                // Clean up
                Timers.clearInterval(id1);
                Timers.clearInterval(id2);
            });
        }

        [Fact]
        public void setInterval_WithArgument_PassesArgument()
        {
            JsEventLoop.Run(async () =>
            {
                var received = new List<string>();
                var id = Timers.setInterval(arguments => received.Add(arguments.Get<string>(0)), 50, "test");
    
                await WaitUntil(() => received.Count >= 2, 1000, $"Expected at least 2 interval executions, got {received.Count}.");
                Timers.clearInterval(id);
    
                Assert.True(received.Count >= 2);
                Assert.All(received, item => Assert.Equal("test", item));
            });
        }

        // ==================== clearInterval Tests ====================

        [Fact]
        public void clearInterval_StopsExecution()
        {
            JsEventLoop.Run(async () =>
            {
                var count = 0;
                var id = Timers.setInterval(_ => count++, 50);
    
                await WaitUntil(() => Volatile.Read(ref count) >= 1, 1000, "Interval did not tick before clearInterval.");
                Timers.clearInterval(id);
                var countAfterClear = count;
    
                await AssertStays(() => Volatile.Read(ref count) == countAfterClear, 250, "clearInterval did not stop further executions.");
                Assert.Equal(countAfterClear, count);
            });
        }

        [Fact]
        public void clearInterval_WithInvalidId_DoesNotThrow()
        {
            JsEventLoop.Run(() =>
            {
                var exception = Record.Exception(() => Timers.clearInterval(99999));
                Assert.Null(exception);
            });
        }

        [Fact]
        public void clearInterval_CalledTwice_DoesNotThrow()
        {
            JsEventLoop.Run(() =>
            {
                var id = Timers.setInterval(_ => { }, 1000);
    
                Timers.clearInterval(id);
                var exception = Record.Exception(() => Timers.clearInterval(id));
                Assert.Null(exception);
            });
        }

        [Fact]
        public void clearInterval_DoesNotRaceWithActiveCallbacks()
        {
            JsEventLoop.Run(async () =>
            {
                for (var iteration = 0; iteration < 200; iteration++)
                {
                    var sawTick = new ManualResetEventSlim(false);
                    var id = Timers.setInterval(_ => sawTick.Set(), 1);
                    Assert.True(await Task.Run(() => sawTick.Wait(1000)));
                    Timers.clearInterval(id);
                    await Task.Delay(5);
                }
            });
        }

        // ==================== Mixed Usage Tests ====================

        [Fact]
        public void setTimeout_And_setInterval_UseUniqueIds()
        {
            JsEventLoop.Run(() =>
            {
                var timeoutId = Timers.setTimeout(_ => { }, 1000);
                var intervalId = Timers.setInterval(_ => { }, 1000);
    
                Assert.NotEqual(timeoutId, intervalId);
    
                Timers.clearTimeout(timeoutId);
                Timers.clearInterval(intervalId);
            });
        }

        [Fact]
        public void clearTimeout_CanClearInterval()
        {
            JsEventLoop.Run(async () =>
            {
                var count = 0;
                var id = Timers.setInterval(_ => count++, 50);
    
                await WaitUntil(() => Volatile.Read(ref count) >= 1, 1000, "Interval did not tick before cross-clear.");
                Timers.clearTimeout(id);
                var countAfterClear = count;
    
                await AssertStays(() => Volatile.Read(ref count) == countAfterClear, 250, "clearTimeout did not stop interval executions.");
                Assert.Equal(countAfterClear, count);
            });
        }

        [Fact]
        public void clearInterval_CanClearTimeout()
        {
            JsEventLoop.Run(async () =>
            {
                using var executed = new ManualResetEventSlim(false);
                var id = Timers.setTimeout(_ => executed.Set(), 100);
    
                Timers.clearInterval(id);
                Assert.False(await Task.Run(() => executed.Wait(300)), "clearInterval did not prevent timeout execution.");
            });
        }

        // ==================== Concurrency Tests ====================

        [Fact]
        public void setTimeout_MultipleTimers_AllExecute()
        {
            JsEventLoop.Run(async () =>
            {
                var results = new bool[3];
                using var allExecuted = new CountdownEvent(3);
    
                Timers.setTimeout(_ =>
                {
                    results[0] = true;
                    allExecuted.Signal();
                }, 30);
                Timers.setTimeout(_ =>
                {
                    results[1] = true;
                    allExecuted.Signal();
                }, 60);
                Timers.setTimeout(_ =>
                {
                    results[2] = true;
                    allExecuted.Signal();
                }, 90);
                Assert.True(await Task.Run(() => allExecuted.Wait(1000)), "Not all timeout callbacks executed within the expected window.");
                Assert.All(results, r => Assert.True(r));
            });
        }

        [Fact]
        public void setInterval_MultipleIntervals_AllExecute()
        {
            JsEventLoop.Run(async () =>
            {
                var counts = new int[2];
    
                var id1 = Timers.setInterval(_ => counts[0]++, 50);
                var id2 = Timers.setInterval(_ => counts[1]++, 50);
    
                await WaitUntil(
                    () => Volatile.Read(ref counts[0]) >= 2 && Volatile.Read(ref counts[1]) >= 2,
                    1000,
                    $"Expected both intervals to execute at least twice, got {counts[0]} and {counts[1]}."
                );
    
                Timers.clearInterval(id1);
                Timers.clearInterval(id2);
    
                Assert.True(counts[0] >= 2);
                Assert.True(counts[1] >= 2);
            });
        }

        // ==================== Edge Cases ====================

        [Fact]
        public void Run_WithEntrypointTask_DrivesTimerBackedAwaitToCompletion()
        {
            var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            JsEventLoop.Run(() =>
            {
                Timers.setTimeout(_ => completion.SetResult(), 0);
                return completion.Task;
            });
            Assert.True(completion.Task.IsCompletedSuccessfully);
        }

        [Fact]
        public void setTimeout_VeryShortDelay_ExecutesQuickly()
        {
            JsEventLoop.Run(async () =>
            {
                using var executed = new ManualResetEventSlim(false);
                Timers.setTimeout(_ => executed.Set(), 1);
    
                await WaitFor(executed, 1000, "Very-short-delay setTimeout did not execute.");
            });
        }

        [Fact]
        public void setInterval_VeryShortInterval_ExecutesMultipleTimes()
        {
            JsEventLoop.Run(async () =>
            {
                var count = 0;
                var id = Timers.setInterval(_ => count++, 10);
    
                await WaitUntil(() => Volatile.Read(ref count) >= 5, 1000, $"Expected at least 5 executions, got {Volatile.Read(ref count)}.");
                Timers.clearInterval(id);
    
                Assert.True(count >= 5, $"Expected at least 5 executions, got {count}");
            });
        }
    }
}
