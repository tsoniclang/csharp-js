using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Tsonic.CSharp.Runtime;
using Xunit;

namespace Tsonic.CSharp.Js.Tests;

[Collection("JavaScript event loop")]
public sealed class PromiseReactionOwnerTests
{
    [Fact]
    public void FinallyPreservesTheOriginalRejectionUnlessItsOwnCallbackThrows()
    {
        var original = new InvalidOperationException("source");
        var replacement = new InvalidOperationException("finally");
        var called = 0;
        Task<int>? generic = null;
        Task? plain = null;
        Task<int>? overridden = null;
        JsEventLoop.Run(() =>
        {
            generic = PromiseRuntime<int>.Finally(Task.FromException<int>(original), () => called++);
            plain = PromiseRuntime.Finally(Task.FromException(original), () => called++);
            overridden = PromiseRuntime<int>.Finally(Task.FromException<int>(original), () => throw replacement);
            Assert.Equal(0, called);
        });
        Assert.Equal(2, called);
        Assert.Same(original, Assert.Throws<InvalidOperationException>(() => generic!.GetAwaiter().GetResult()));
        Assert.Same(original, Assert.Throws<InvalidOperationException>(() => plain!.GetAwaiter().GetResult()));
        Assert.Same(replacement, Assert.Throws<InvalidOperationException>(() => overridden!.GetAwaiter().GetResult()));
    }

    [Fact]
    public void ShutdownDoesNotExecuteEitherKindOfPendingFinallyCallback()
    {
        var source = new TaskCompletionSource<int>();
        var called = 0;
        Task<int>? generic = null;
        Task? plain = null;
        JsEventLoop.Run(() =>
        {
            generic = PromiseRuntime<int>.Finally(source.Task, () => called++);
            plain = PromiseRuntime.Finally(source.Task, () => called++);
        });
        Assert.True(generic!.IsCanceled);
        Assert.True(plain!.IsCanceled);
        JsEventLoop.Run(() => source.SetResult(7));
        Assert.Equal(0, called);
    }

    [Fact]
    public void OwnedCompletionPreservesSettlementOrderRatherThanRegistrationOrder()
    {
        var seen = new List<string>();
        JsEventLoop.Run(() =>
        {
            TaskResolve<int>? resolveFirst = null;
            TaskReject? rejectSecond = null;
            var first = TaskCompletion<int>.Create((resolve, _) => resolveFirst = resolve);
            var second = TaskCompletion.Create((_, reject) => rejectSecond = reject);
            _ = PromiseRuntime<int>.Then(first, _ => seen.Add("first"));
            _ = PromiseRuntime.Catch(second, _ => seen.Add("second"));
            rejectSecond!(TsValue.from("reason"));
            resolveFirst!(42);
            Assert.Empty(seen);
            seen.Add("entry");
        });
        Assert.Equal(new[] { "entry", "second", "first" }, seen);
    }

    [Fact]
    public void PendingOwnedTaskAwaitMatchesTheNativeAsynchronousContinuation()
    {
        var actual = new List<string>();
        var native = new List<string>();
        JsEventLoop.Run(() =>
        {
            TaskResolve? resolve = null;
            var source = TaskCompletion.Create((complete, _) => resolve = complete);
            _ = Await(source, actual);
            actual.Add("before resolve");
            resolve!();
            actual.Add("after resolve");
        });
        JsEventLoop.Run(() =>
        {
            var source = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            _ = Await(source.Task, native);
            native.Add("before resolve");
            source.SetResult();
            native.Add("after resolve");
        });
        Assert.Equal(new[] { "before resolve", "after resolve", "resumed" }, actual);
        Assert.Equal(native, actual);

        static async Task Await(Task source, List<string> seen) { await source; seen.Add("resumed"); }
    }

    [Fact]
    public async Task SettledNativeValuesAndDirectAwaitRequireNoJavascriptOwner()
    {
        const ulong value = 9007199254740993UL;
        var source = PromiseRuntime<ulong>.Resolved(value);
        Assert.Equal(value, await source);
        Assert.Same(source, PromiseRuntime<ulong>.Resolve(source));
        Assert.Equal(value, await Native());
        static async Task<ulong> Native() => await Task.FromResult(value);
    }

    [Fact]
    public void ReactionWithoutAnOwnerRejectsWithoutInvokingEitherCallback()
    {
        var invoked = false;
        var pending = PromiseRuntime<int>.Then(Task.FromResult(1),
            value => { invoked = true; return value; }, _ => { invoked = true; return 0; });
        var failure = Assert.Throws<InvalidOperationException>(() => pending.GetAwaiter().GetResult());
        Assert.Equal("No JavaScript event loop is running on this thread.", failure.Message);
        Assert.False(invoked);
    }

    [Fact]
    public void AllSettledReactionKindsAreDeferredAndNestedReactionsStayFifo()
    {
        var values = new List<string>();
        var original = new InvalidOperationException("original");
        JsEventLoop.Run(() =>
        {
            _ = PromiseRuntime<int>.Then(Task.FromResult(7), value =>
            {
                values.Add("then " + value);
                _ = PromiseRuntime.Finally(Task.CompletedTask, () => values.Add("nested"));
                return value;
            });
            _ = PromiseRuntime.Catch(Task.FromException(original), reason => { Assert.Same(original, reason); values.Add("catch"); });
            _ = PromiseRuntime.Finally(Task.CompletedTask, () => values.Add("finally"));
            Assert.Empty(values);
            values.Add("entry");
        });
        Assert.Equal(new[] { "entry", "then 7", "catch", "finally", "nested" }, values);
    }

    [Fact]
    public void PendingNativeSourceAndRejectedCallbackPreserveOwnerAndIdentity()
    {
        var owner = Environment.CurrentManagedThreadId;
        var reason = new object();
        JsEventLoop.Run(async () =>
        {
            var source = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            var pending = PromiseRuntime<int>.Then(source.Task, value => value, received =>
            {
                Assert.Equal(owner, Environment.CurrentManagedThreadId);
                Assert.Same(reason, received);
                return 7;
            });
            await Task.Run(() => source.SetException(TsThrownValueException.from(reason)));
            Assert.Equal(7, await pending);
        });
    }

    [Fact]
    public void CallbackFailureDoesNotInvokeItsOwnRejectionHandler()
    {
        var original = new InvalidOperationException("callback");
        var rejected = false;
        JsEventLoop.Run(async () =>
        {
            var pending = PromiseRuntime<int>.Then<int>(Task.FromResult(1), _ => throw original,
                _ => { rejected = true; return 0; });
            Assert.Same(original, await Assert.ThrowsAsync<InvalidOperationException>(() => pending));
        });
        Assert.False(rejected);
    }

    [Fact]
    public void TimerBackedDiscardedReactionCompletesBeforeIdleExit()
    {
        for (var iteration = 0; iteration < 32; iteration++)
        {
            var completed = false;
            JsEventLoop.Run(() =>
            {
                var source = new TaskCompletionSource<int>();
                _ = PromiseRuntime<int>.Then(source.Task, _ => completed = true);
                Timers.setTimeout(_ => source.SetResult(1), 1);
            });
            Assert.True(completed);
        }
    }

    [Fact]
    public void RejectedDiscardedCallReportsAfterCurrentJobAndSetsNativeExitCode()
    {
        var previousError = Console.Error;
        var previousExit = Environment.ExitCode;
        using var reported = new StringWriter();
        var afterCall = false;
        var laterJob = false;
        try
        {
            Console.SetError(reported);
            JsEventLoop.Run(() =>
            {
                PromiseRuntime.ObserveDiscarded(PromiseRuntime.Reject("exact rejection"));
                Assert.Equal(string.Empty, reported.ToString());
                afterCall = true;
                JsEventLoop.Enqueue(() => laterJob = true);
            });
            Assert.True(afterCall);
            Assert.False(laterJob);
            Assert.Equal(1, Environment.ExitCode);
            Assert.Equal("exact rejection" + Environment.NewLine, reported.ToString());
        }
        finally { Console.SetError(previousError); Environment.ExitCode = previousExit; }
    }

    [Fact]
    public void SuccessfulDiscardedObserverDoesNotPostAJobOrAllocateAnObject()
    {
        PromiseRuntime.ObserveDiscarded(Task.CompletedTask);
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var index = 0; index < 4096; index++) PromiseRuntime.ObserveDiscarded(Task.CompletedTask);
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - before);
    }

    [Fact]
    public void AnUnobservedStoredRejectionDoesNotAcquireTheDiscardedObserverPolicy()
    {
        var previousExit = Environment.ExitCode;
        JsEventLoop.Run(() => { _ = PromiseRuntime.Reject("stored"); });
        Assert.Equal(previousExit, Environment.ExitCode);
    }
}
