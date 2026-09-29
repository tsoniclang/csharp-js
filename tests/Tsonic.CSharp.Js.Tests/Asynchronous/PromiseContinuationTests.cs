using System;
using System.Threading.Tasks;
using Xunit;

namespace Tsonic.CSharp.Js.Tests;

public class PromiseContinuationTests
{
    [Fact]
    public async Task Then_PreservesExactNativeValuesAndAdoptsTasks()
    {
        const ulong value = 9007199254740993UL;
        Assert.Equal(value, await PromiseRuntime<ulong>.Then(Task.FromResult(value), received => received));
        Assert.Equal(value, await PromiseRuntime<ulong>.ThenAsync(Task.FromResult(value), received => Task.FromResult(received)));
        Assert.Equal(value, await PromiseRuntime.Then(Task.CompletedTask, () => value));
    }

    [Fact]
    public async Task Then_DiscardedContinuationStillRuns()
    {
        var source = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var reached = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _ = PromiseRuntime.Then(source.Task, () => reached.SetResult());
        source.SetResult();
        await reached.Task.WaitAsync(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Then_ThrownCallbackDoesNotCallItsOwnRejectionHandler()
    {
        var expected = new InvalidOperationException("callback");
        var rejectedCalls = 0;
        var result = PromiseRuntime<int>.Then<int>(Task.FromResult(1), _ => throw expected, _ =>
        {
            rejectedCalls++;
            return 0;
        });
        Assert.Same(expected, await Assert.ThrowsAsync<InvalidOperationException>(async () => await result));
        Assert.Equal(0, rejectedCalls);
    }

    [Fact]
    public async Task Catch_ReceivesTheOriginalRejectionAndPreservesSuccess()
    {
        var reason = new object();
        var result = PromiseRuntime<int>.Catch(PromiseRuntime<int>.Reject(reason), received =>
        {
            Assert.Same(reason, received);
            return 7;
        });
        Assert.Equal(7, await result);
        Assert.Equal(3, await PromiseRuntime<int>.Catch(Task.FromResult(3), _ => throw new Exception("unexpected callback")));
        var expected = new InvalidOperationException("native exception");
        await PromiseRuntime.Catch(PromiseRuntime.Reject(expected), received => Assert.Same(expected, received));
    }
}
