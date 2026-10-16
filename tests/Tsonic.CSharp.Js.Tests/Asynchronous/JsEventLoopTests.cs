using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tsonic.CSharp.Js.Tests;

[Collection("JavaScript event loop")]
public class JsEventLoopTests
{
    [Fact]
    public void EntryFactoryOwnsDiscardedCompletedPromiseContinuation()
    {
        var previous = SynchronizationContext.Current;
        var owner = Environment.CurrentManagedThreadId;
        var invoked = false;
        JsEventLoop.Run(() =>
        {
            Assert.NotSame(previous, SynchronizationContext.Current);
            _ = PromiseRuntime.Then(Task.CompletedTask, () =>
            {
                Assert.Equal(owner, Environment.CurrentManagedThreadId);
                invoked = true;
            });
            Assert.False(invoked);
        });
        Assert.True(invoked);
        Assert.Same(previous, SynchronizationContext.Current);
        Assert.False(ProcessKeepAlive.HasReferences);
    }

    [Fact]
    public void AsyncEntryAndPromiseCallbacksStayOnTheirNativeLoopOwner()
    {
        var owner = Environment.CurrentManagedThreadId;
        var reached = false;
        JsEventLoop.Run(async () =>
        {
            await Task.Delay(1);
            Assert.Equal(owner, Environment.CurrentManagedThreadId);
            await PromiseRuntime<int>.Then(Task.FromResult(7), value =>
            {
                Assert.Equal(owner, Environment.CurrentManagedThreadId);
                Assert.Equal(7, value);
                reached = true;
            });
        });
        Assert.True(reached);
        Assert.False(ProcessKeepAlive.HasReferences);
    }

    [Fact]
    public void CatchAndFinallyUseTheSameDeferredNativeOwner()
    {
        var owner = Environment.CurrentManagedThreadId;
        var recovered = false;
        var finalized = false;
        JsEventLoop.Run(() =>
        {
            _ = PromiseRuntime.Catch(Task.FromException(new InvalidOperationException("original")), _ =>
            {
                Assert.Equal(owner, Environment.CurrentManagedThreadId);
                recovered = true;
            });
            _ = PromiseRuntime.Finally(Task.CompletedTask, () =>
            {
                Assert.Equal(owner, Environment.CurrentManagedThreadId);
                finalized = true;
            });
            Assert.False(recovered);
            Assert.False(finalized);
        });
        Assert.True(recovered);
        Assert.True(finalized);
        Assert.False(ProcessKeepAlive.HasReferences);
    }

    [Fact]
    public void PendingPromiseAloneDoesNotManufactureProcessLiveness()
    {
        var source = new TaskCompletionSource();
        Task? continuation = null;
        var invoked = false;
        JsEventLoop.Run(() =>
        {
            continuation = PromiseRuntime.Then(source.Task, () =>
            {
                invoked = true;
                Assert.Fail("Pending Promise was not settled.");
            });
        });
        Assert.NotNull(continuation);
        Assert.True(continuation.IsCanceled);
        Assert.False(source.Task.IsCompleted);
        source.SetResult();
        Assert.False(invoked);
        Assert.False(ProcessKeepAlive.HasReferences);
    }

    [Fact]
    public void EntryFailureRestoresContextAndPreservesNativeExceptionIdentity()
    {
        var previous = SynchronizationContext.Current;
        var expected = new InvalidOperationException("entry");
        Assert.Same(expected, Assert.Throws<InvalidOperationException>(() => JsEventLoop.Run(() => Task.FromException(expected))));
        Assert.Same(previous, SynchronizationContext.Current);
        Assert.False(ProcessKeepAlive.HasReferences);
    }

    [Fact]
    public void NativeContinuationPostingDoesNotAllocateAnAdapterPerCallback()
    {
        var payload = new PostingCounter();
        SendOrPostCallback callback = static state => ((PostingCounter)state!).Count++;
        JsEventLoop.Run(() =>
        {
            var context = SynchronizationContext.Current!;
            for (var index = 0; index < 4096; index++) context.Post(callback, payload);
        });
        Assert.Equal(4096, payload.Count);
        var allocated = 0L;
        JsEventLoop.Run(() =>
        {
            var context = SynchronizationContext.Current!;
            var before = GC.GetAllocatedBytesForCurrentThread();
            for (var index = 0; index < 4096; index++) context.Post(callback, payload);
            allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        });
        Assert.Equal(8192, payload.Count);
        Assert.Equal(0, allocated);
        Assert.False(ProcessKeepAlive.HasReferences);
    }

    private sealed class PostingCounter
    {
        public int Count;
    }
}
