using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tsonic.CSharp.Js.Tests;

[Collection("JavaScript event loop")]
public sealed class LoopOwnershipTests
{
    [Fact]
    public void MissingOwnerRejectsAdmissionWithoutExecutingTheCallback()
    {
        var invoked = false;
        var failure = Assert.Throws<InvalidOperationException>(() => JsEventLoop.Enqueue(() => invoked = true));
        Assert.Equal("No JavaScript event loop is running on this thread.", failure.Message);
        Assert.False(invoked);
        Assert.Throws<InvalidOperationException>(() => Timers.setTimeout(_ => invoked = true));
        Assert.False(ProcessKeepAlive.HasReferences);
    }

    [Fact]
    public void ReentrantAdmissionPreservesFifoAndDoesNotCreateResourceReferences()
    {
        var seen = new List<int>();
        JsEventLoop.Run(() =>
        {
            JsEventLoop.Enqueue(() => { seen.Add(1); JsEventLoop.Enqueue(() => seen.Add(3)); });
            JsEventLoop.Enqueue(() => seen.Add(2));
            Assert.False(ProcessKeepAlive.HasReferences);
            Assert.Empty(seen);
        });
        Assert.Equal(new[] { 1, 2, 3 }, seen);
    }

    [Fact]
    public void FailureClosesOnlyItsGenerationAndPreservesOriginalExceptionIdentity()
    {
        var original = new InvalidOperationException("original");
        var old = default(LoopDispatch);
        var called = false;
        var stopped = false;
        Assert.Same(original, Assert.Throws<InvalidOperationException>(() => JsEventLoop.Run(() =>
        {
            old = JsEventLoop.Capture();
            old.Register(() => stopped = true);
            old.Enqueue(() => throw original);
            old.Enqueue(() => called = true);
        })));
        Assert.True(stopped);
        Assert.False(called);
        JsEventLoop.Run(() =>
        {
            Assert.False(old.IsActive);
            Assert.False(old.TryEnqueue(() => called = true));
            JsEventLoop.Enqueue(static () => { });
        });
        Assert.False(called);
    }

    [Fact]
    public void NativeCapturedSynchronizationContextCannotPostIntoTheNextActivation()
    {
        var source = new TaskCompletionSource();
        SynchronizationContext? old = null;
        var resumed = false;
        JsEventLoop.Run(() =>
        {
            old = SynchronizationContext.Current;
            _ = NativeBody();
        });
        JsEventLoop.Run(() =>
        {
            Assert.NotSame(old, SynchronizationContext.Current);
            source.SetResult();
            old!.Post(_ => resumed = true, null);
            Assert.Throws<InvalidOperationException>(() => old.Send(static _ => { }, null));
        });
        Assert.False(resumed);

        async Task NativeBody()
        {
            await source.Task;
            resumed = true;
        }
    }

    [Fact]
    public void ShutdownCancelsAnOwnedPendingReactionWithoutCallingEitherUserBranch()
    {
        var source = new TaskCompletionSource<int>();
        Task<int>? pending = null;
        var called = false;
        JsEventLoop.Run(() => { pending = PromiseRuntime<int>.Then(source.Task,
            _ => { called = true; return 1; }, _ => { called = true; return 2; }); });
        Assert.NotNull(pending);
        Assert.True(pending.IsCanceled);
        JsEventLoop.Run(() => source.SetResult(7));
        Assert.False(called);
    }

    [Fact]
    public void ShutdownReleasesTheReactionCaptureWhileItsExternalNativeTaskRemainsPending()
    {
        var source = new TaskCompletionSource<int>();
        var capture = CreatePendingCapture(source.Task);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        Assert.False(capture.IsAlive);
        Assert.False(source.Task.IsCompleted);
        GC.KeepAlive(source);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference CreatePendingCapture(Task<int> source)
    {
        var payload = new CapturePayload();
        var reference = new WeakReference(payload);
        JsEventLoop.Run(() => { _ = PromiseRuntime<int>.Then(source, value => value + payload.Value); });
        return reference;
    }

    private sealed class CapturePayload { internal int Value = 7; }

    [Fact]
    public void RootAwaitAndForeignIngressKeepUserCodeOnTheOwner()
    {
        var owner = Environment.CurrentManagedThreadId;
        var observed = false;
        JsEventLoop.Run(async () =>
        {
            var dispatch = JsEventLoop.Capture();
            var completed = new TaskCompletionSource();
            await Task.Run(() => dispatch.Enqueue(() =>
            {
                Assert.Equal(owner, Environment.CurrentManagedThreadId);
                observed = true;
                completed.SetResult();
            }));
            await completed.Task;
            Assert.Equal(owner, Environment.CurrentManagedThreadId);
        });
        Assert.True(observed);
    }

    [Fact]
    public void ReadyLocalIngressAndRegistrationLimitsRejectBeforePartialAdmission()
    {
        var calls = 0;
        JsEventLoop.Run(() =>
        {
            var dispatch = JsEventLoop.Capture();
            dispatch.Enqueue(() => calls++);
            Assert.False(dispatch.TryEnqueue(() => calls++));
            Assert.Contains("finite event-loop limit", Assert.Throws<InvalidOperationException>(() => dispatch.Enqueue(() => calls++)).Message);
            using var registration = dispatch.Register(static () => { });
            Assert.Throws<InvalidOperationException>(() => dispatch.Register(static () => { }));
            registration.Dispose();
            using var reused = dispatch.Register(static () => { });
        }, 1, 1, 1);
        Assert.Equal(1, calls);
    }

    [Fact]
    public void ForeignIngressHasItsOwnFiniteReservationAndReleasesItAfterDrain()
    {
        var calls = 0;
        JsEventLoop.Run(() =>
        {
            var dispatch = JsEventLoop.Capture();
            var first = false;
            var second = true;
            var producer = new Thread(() =>
            {
                first = dispatch.TryEnqueue(() => calls++);
                second = dispatch.TryEnqueue(() => calls++);
            });
            producer.Start();
            Assert.True(producer.Join(TimeSpan.FromSeconds(5)));
            Assert.True(first);
            Assert.False(second);
        }, 1, 1, 2);
        Assert.Equal(1, calls);
    }

    [Fact]
    public void PendingReactionAndForeignResourceShareOneFiniteAdmissionLimit()
    {
        for (var iteration = 0; iteration < 64; iteration++)
        {
            using var start = new ManualResetEventSlim();
            var source = new TaskCompletionSource<int>();
            var foreignAdmitted = false;
            var localAdmitted = false;
            Exception? failure = null;
            var registration = default(LoopRegistration);
            JsEventLoop.Run(() =>
            {
                var dispatch = JsEventLoop.Capture();
                var foreign = new Thread(() =>
                {
                    try
                    {
                        if (!start.Wait(TimeSpan.FromSeconds(5))) throw new TimeoutException();
                        registration = dispatch.Register(static () => { });
                        foreignAdmitted = true;
                    }
                    catch (InvalidOperationException error) when (error.Message.Contains("registration count")) { }
                    catch (Exception error) { failure = error; }
                });
                foreign.Start();
                start.Set();
                try { PromiseRuntime.ObserveDiscarded(source.Task); localAdmitted = true; }
                catch (InvalidOperationException error) when (error.Message.Contains("registration count")) { }
                Assert.True(foreign.Join(TimeSpan.FromSeconds(5)));
                Assert.Null(failure);
                Assert.NotEqual(foreignAdmitted, localAdmitted);
                registration.Dispose();
            }, 8, 8, 1);
        }
    }

    [Fact]
    public void CompletingAPendingReactionReleasesItsAdmissionBeforeTheUserCallback()
    {
        var source = new TaskCompletionSource<int>();
        var called = false;
        JsEventLoop.Run(() =>
        {
            var dispatch = JsEventLoop.Capture();
            _ = PromiseRuntime<int>.Then(source.Task, value =>
            {
                using var reused = dispatch.Register(static () => { });
                called = true;
                return value;
            });
            Assert.Throws<InvalidOperationException>(() => dispatch.Register(static () => { }));
            source.SetResult(1);
        }, 8, 8, 1);
        Assert.True(called);
    }

    [Fact]
    public void OldRegistrationDisposalCannotReleaseTheNextEpochsAdmission()
    {
        var old = default(LoopRegistration);
        JsEventLoop.Run(() => old = JsEventLoop.Capture().Register(static () => { }), 8, 8, 1);
        JsEventLoop.Run(() =>
        {
            var dispatch = JsEventLoop.Capture();
            using var current = dispatch.Register(static () => { });
            old.Dispose();
            Assert.Throws<InvalidOperationException>(() => dispatch.Register(static () => { }));
        }, 8, 8, 1);
    }

    [Fact]
    public void ForeignReferencedResourceReleaseWakesItsWaitingOwner()
    {
        using var admitted = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        Exception? failure = null;
        Thread? producer = null;
        try
        {
            JsEventLoop.Run(() =>
            {
                var dispatch = JsEventLoop.Capture();
                producer = new Thread(() =>
                {
                    try
                    {
                        using var registration = dispatch.Register(static () => { }, referenced: true);
                        admitted.Set();
                        if (!release.Wait(TimeSpan.FromSeconds(5))) throw new TimeoutException();
                    }
                    catch (Exception error) { failure = error; admitted.Set(); }
                });
                producer.Start();
                Assert.True(admitted.Wait(TimeSpan.FromSeconds(5)));
                Assert.Null(failure);
                Assert.True(ProcessKeepAlive.HasReferences);
                dispatch.Enqueue(release.Set);
            });
        }
        finally
        {
            release.Set();
            if (producer is not null) Assert.True(producer.Join(TimeSpan.FromSeconds(5)));
        }
        Assert.Null(failure);
        Assert.False(ProcessKeepAlive.HasReferences);
    }

    [Fact]
    public void InvalidLimitsDoNotActivateAnOwner()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => JsEventLoop.Run(static () => { }, 0, 1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => JsEventLoop.Run(static () => { }, 1, 0, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => JsEventLoop.Run(static () => { }, 1, 1, 0));
        Assert.Throws<InvalidOperationException>(() => JsEventLoop.Capture());
    }

    [Fact]
    public void RegisteredPhaseIdentityAndOrderingAreExplicitAndValidated()
    {
        var first = new PhaseRegistration().Phase;
        var second = new PhaseRegistration().Phase;
        var other = new PhaseRegistration().Phase;
        var values = new List<int>();
        JsEventLoop.Run(() =>
        {
            Assert.Throws<ArgumentException>(() => JsEventLoop.RegisterPhase(first, [first, first]));
            Assert.Throws<ArgumentException>(() => JsEventLoop.RegisterPhase(default, [first]));
            Assert.Throws<ArgumentException>(() => JsEventLoop.RegisterPhase(other, [first, second]));
            var secondDispatch = JsEventLoop.RegisterPhase(second, [first, second]);
            var firstDispatch = JsEventLoop.RegisterPhase(first, [first, second]);
            Assert.Throws<InvalidOperationException>(() => JsEventLoop.RegisterPhase(first, [second, first]));
            secondDispatch.Enqueue(() => values.Add(2));
            firstDispatch.Enqueue(() => values.Add(1));
            JsEventLoop.Enqueue(() => values.Add(0));
        });
        Assert.Equal(new[] { 0, 1, 2 }, values);
    }

    [Fact]
    public void ShutdownCallsResourceOwnersOutsideTheIngressLock()
    {
        var closed = false;
        JsEventLoop.Run(() =>
        {
            var dispatch = JsEventLoop.Capture();
            dispatch.Register(() =>
            {
                var producer = new Thread(() =>
                {
                    Assert.False(dispatch.TryEnqueue(static () => { }));
                    closed = true;
                });
                producer.Start();
                Assert.True(producer.Join(TimeSpan.FromSeconds(5)));
            });
        });
        Assert.True(closed);
    }

    [Fact]
    public void ConcurrentOwnersDoNotResetEachOthersResourcesOrIngress()
    {
        using var arrived = new CountdownEvent(2);
        var firstRoot = new TaskCompletionSource();
        var secondRoot = new TaskCompletionSource();
        var firstClosed = false;
        var secondClosed = false;
        var secondDispatch = default(LoopDispatch);
        Exception? firstFailure = null;
        Exception? secondFailure = null;
        var first = new Thread(() =>
        {
            try { JsEventLoop.Run(() => { JsEventLoop.Capture().Register(() => firstClosed = true); arrived.Signal(); return firstRoot.Task; }); }
            catch (Exception exception) { firstFailure = exception; }
        });
        var second = new Thread(() =>
        {
            try { JsEventLoop.Run(() => { secondDispatch = JsEventLoop.Capture(); secondDispatch.Register(() => secondClosed = true); arrived.Signal(); return secondRoot.Task; }); }
            catch (Exception exception) { secondFailure = exception; }
        });
        first.Start();
        second.Start();
        try
        {
            Assert.True(arrived.Wait(TimeSpan.FromSeconds(5)));
            firstRoot.SetResult();
            Assert.True(first.Join(TimeSpan.FromSeconds(5)));
            Assert.True(firstClosed);
            Assert.True(second.IsAlive);
            Assert.True(secondDispatch.IsActive);
            Assert.False(secondClosed);
        }
        finally
        {
            firstRoot.TrySetResult();
            secondRoot.TrySetResult();
            Assert.True(first.Join(TimeSpan.FromSeconds(5)));
            Assert.True(second.Join(TimeSpan.FromSeconds(5)));
        }
        Assert.Null(firstFailure);
        Assert.Null(secondFailure);
        Assert.True(secondClosed);
    }
}
