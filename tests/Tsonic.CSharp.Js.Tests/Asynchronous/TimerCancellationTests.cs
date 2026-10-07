using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tsonic.CSharp.Js.Tests;

[Collection("JavaScript event loop")]
public sealed class TimerCancellationTests
{
    [Theory]
    [InlineData(Timeout.Infinite)]
    [InlineData(1)]
    public void CancellationSuppressesAlreadyQueuedCallbacks(int period)
    {
        var calls = 0;
        using var handle = new Timers.TimerHandle(0, _ => calls++, [], period);
        handle.QueueCallback();
        handle.QueueCallback();
        Assert.True(ProcessKeepAlive.HasReferences);

        handle.Dispose();
        handle.Dispose();
        Assert.True(handle.IsDisposed);
        Assert.False(ProcessKeepAlive.HasReferences);
        JsEventLoop.Run();
        Assert.Equal(0, calls);
        Assert.False(ProcessKeepAlive.HasReferences);
    }

    [Theory]
    [InlineData(Timeout.Infinite)]
    [InlineData(1)]
    public void CancellationBeforeArmCannotRestartOrInvokeTheNativeTimer(int period)
    {
        using var handle = new Timers.TimerHandle(0,
            _ => throw new InvalidOperationException("cancelled timer executed"), [], period);
        handle.Dispose();
        handle.Start(0);
        handle.QueueCallback();
        JsEventLoop.Run();
        Assert.True(handle.IsDisposed);
        Assert.False(ProcessKeepAlive.HasReferences);
    }

    [Theory]
    [InlineData(Timeout.Infinite)]
    [InlineData(60_000)]
    public async Task NativeArmingAndCancellationHaveOneSynchronizedOwner(int period)
    {
        for (var iteration = 0; iteration < 256; iteration++)
        {
            using var handle = new Timers.TimerHandle(0,
                _ => throw new InvalidOperationException("cancelled timer executed"), [], period);
            using var boundary = new Barrier(2);
            var starting = Task.Run(() =>
            {
                Assert.True(boundary.SignalAndWait(10_000));
                handle.Start(60_000);
            });
            var cancelling = Task.Run(() =>
            {
                Assert.True(boundary.SignalAndWait(10_000));
                handle.Dispose();
            });
            await Task.WhenAll(starting, cancelling).WaitAsync(TimeSpan.FromSeconds(10));
            handle.Start(0);
            handle.QueueCallback();
            Assert.True(handle.IsDisposed);
            Assert.False(ProcessKeepAlive.HasReferences);
        }
        JsEventLoop.Run();
    }

    [Fact]
    public async Task ClearingAStartedCallbackDoesNotInterruptItsNativeExecution()
    {
        using var entered = new ManualResetEventSlim();
        using var resume = new ManualResetEventSlim();
        var completed = false;
        using var handle = new Timers.TimerHandle(0, _ =>
        {
            entered.Set();
            Assert.True(resume.Wait(10_000));
            completed = true;
        }, [], Timeout.Infinite);
        handle.QueueCallback();
        var running = Task.Run(JsEventLoop.Run);
        try
        {
            Assert.True(entered.Wait(10_000));
            handle.Dispose();
            Assert.False(completed);
            Assert.False(ProcessKeepAlive.HasReferences);
        }
        finally
        {
            resume.Set();
            await running.WaitAsync(TimeSpan.FromSeconds(10));
        }
        Assert.True(completed);
        Assert.False(ProcessKeepAlive.HasReferences);
    }

    [Fact]
    public void SelfCancellationSuppressesOtherQueuedIntervalInvocations()
    {
        var calls = 0;
        Timers.TimerHandle? handle = null;
        using (handle = new Timers.TimerHandle(0, _ =>
        {
            calls++;
            handle!.Dispose();
        }, [], 1))
        {
            handle.QueueCallback();
            handle.QueueCallback();
            JsEventLoop.Run();
            Assert.Equal(1, calls);
            Assert.True(handle.IsDisposed);
            Assert.False(ProcessKeepAlive.HasReferences);
        }
    }

    [Fact]
    public void FailureReleasesItsHandleAndRetainsUninvokedTimerWork()
    {
        var original = new InvalidOperationException("original queued timer failure");
        var completed = false;
        using var failing = new Timers.TimerHandle(0, _ => throw original, [], Timeout.Infinite);
        using var later = new Timers.TimerHandle(0, _ => completed = true, [], Timeout.Infinite);
        failing.QueueCallback();
        later.QueueCallback();
        Assert.Same(original, Assert.Throws<InvalidOperationException>(JsEventLoop.Run));
        Assert.False(completed);
        Assert.True(failing.IsDisposed);
        Assert.False(later.IsDisposed);
        Assert.True(ProcessKeepAlive.HasReferences);
        JsEventLoop.Run();
        Assert.True(completed);
        Assert.True(later.IsDisposed);
        Assert.False(ProcessKeepAlive.HasReferences);
    }

    [Fact]
    public void CancelledDispatchAllocatesNoArgumentsOrCallbackCopies()
    {
        JsEventLoop.Run();
        using var handle = new Timers.TimerHandle(0,
            _ => throw new InvalidOperationException("cancelled timer executed"), [new object()], 1);
        handle.QueueCallback();
        handle.QueueCallback();
        handle.Dispose();
        var before = GC.GetAllocatedBytesForCurrentThread();
        JsEventLoop.Run();
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - before);
        Assert.False(ProcessKeepAlive.HasReferences);
    }

    [Fact]
    public void NativeDispatchAllocatesOnlyTheExactCallbackArguments()
    {
        TimerCallback callback = _ => { };
        foreach (var arguments in new object?[][] { [], ["payload"], [1, "payload", null] })
        {
            using (var warmup = new Timers.TimerHandle(0, callback, arguments, Timeout.Infinite))
            {
                warmup.QueueCallback();
                JsEventLoop.Run();
            }
            callback(new TimerCallbackArguments(arguments));

            for (var iteration = 0; iteration < 32; iteration++)
            {
                using var handle = new Timers.TimerHandle(0, callback, arguments, Timeout.Infinite);
                handle.QueueCallback();
                var actualBefore = GC.GetAllocatedBytesForCurrentThread();
                JsEventLoop.Run();
                var actual = GC.GetAllocatedBytesForCurrentThread() - actualBefore;
                var expectedBefore = GC.GetAllocatedBytesForCurrentThread();
                callback(new TimerCallbackArguments(arguments));
                var expected = GC.GetAllocatedBytesForCurrentThread() - expectedBefore;
                Assert.Equal(expected, actual);
                Assert.False(ProcessKeepAlive.HasReferences);
            }
        }
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-1.0)]
    [InlineData(0.5)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public async Task NormalizedZeroIntervalsStillRepeatUntilCleared(double delay)
    {
        var calls = 0;
        var id = 0;
        id = Timers.setInterval(_ =>
        {
            if (++calls == 3)
            {
                Timers.clearInterval(id);
            }
        }, delay);
        var running = Task.Run(JsEventLoop.Run);
        try
        {
            await running.WaitAsync(TimeSpan.FromSeconds(10));
        }
        finally
        {
            Timers.clearInterval(id);
            await running.WaitAsync(TimeSpan.FromSeconds(10));
        }
        Assert.Equal(3, calls);
        Assert.False(ProcessKeepAlive.HasReferences);
    }

    [Fact]
    public void RepeatedCancellationCannotReleaseAnotherNativeTimerOwner()
    {
        var completed = false;
        using var cancelled = new Timers.TimerHandle(0,
            _ => throw new InvalidOperationException("cancelled timer executed"), [], Timeout.Infinite);
        using var retained = new Timers.TimerHandle(0, _ => completed = true, [], Timeout.Infinite);
        cancelled.QueueCallback();
        cancelled.Dispose();
        cancelled.Dispose();
        cancelled.Start(0);
        Assert.True(ProcessKeepAlive.HasReferences);
        retained.QueueCallback();
        JsEventLoop.Run();
        Assert.True(completed);
        Assert.False(ProcessKeepAlive.HasReferences);
    }
}
