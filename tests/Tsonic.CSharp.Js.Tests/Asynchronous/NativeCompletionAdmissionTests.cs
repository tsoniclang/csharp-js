using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Tsonic.CSharp.Js.Tests;

[Collection("JavaScript event loop")]
public sealed class NativeCompletionAdmissionTests
{
    [Fact]
    public void SettledNativeTaskIsDeliveredBeforeIdleEvenWhenItsAsyncNotificationRunsLater()
    {
        var invoked = 0;
        var reactions = new Task[256];
        for (var iteration = 0; iteration < reactions.Length; iteration++)
        {
            JsEventLoop.Run(() =>
            {
                var source = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
                reactions[iteration] = PromiseRuntime<int>.Then(source.Task, _ => invoked++);
                JsEventLoop.Enqueue(() => source.SetResult(17));
            });
        }
        Assert.Equal(reactions.Length, invoked);
        Assert.All(reactions, reaction => Assert.True(reaction.IsCompletedSuccessfully));
        JsEventLoop.Run(static () => { });
        Assert.Equal(reactions.Length, invoked);
    }

    [Fact]
    public void SettledDiscardedNativeRejectionIsReportedBeforeIdleWithoutKeepingPendingTasksAlive()
    {
        var previousError = Console.Error;
        var previousExit = Environment.ExitCode;
        using var error = new StringWriter();
        try
        {
            Console.SetError(error);
            for (var iteration = 0; iteration < 256; iteration++) JsEventLoop.Run(() =>
            {
                var source = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                PromiseRuntime.ObserveDiscarded(source.Task);
                JsEventLoop.Enqueue(() => source.SetException(new InvalidOperationException("native rejection")));
            });
            Assert.Equal(1, Environment.ExitCode);
            Assert.Equal(string.Concat(System.Linq.Enumerable.Repeat("native rejection" + Environment.NewLine, 256)), error.ToString());
        }
        finally { Console.SetError(previousError); Environment.ExitCode = previousExit; }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ClosedDiscardedObservationDoesNotReportInAnotherGeneration(bool completeInsideNextLoop)
    {
        var previousError = Console.Error;
        var previousExit = Environment.ExitCode;
        using var error = new StringWriter();
        try
        {
            Console.SetError(error);
            var source = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            var original = new InvalidOperationException("late original rejection");
            JsEventLoop.Run(() => PromiseRuntime.ObserveDiscarded(source.Task));
            if (completeInsideNextLoop)
                JsEventLoop.Run(() => source.SetException(original));
            else
                source.SetException(original);
            Assert.Equal(previousExit, Environment.ExitCode);
            Assert.Equal(string.Empty, error.ToString());
            Assert.Same(original, Assert.Throws<InvalidOperationException>(() => source.Task.GetAwaiter().GetResult()));
        }
        finally { Console.SetError(previousError); Environment.ExitCode = previousExit; }
    }

    [Fact]
    public void PendingNativeTaskCanBeExplicitlyObservedInTheNextGeneration()
    {
        var previousError = Console.Error;
        var previousExit = Environment.ExitCode;
        using var error = new StringWriter();
        try
        {
            Console.SetError(error);
            var source = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            JsEventLoop.Run(() => PromiseRuntime.ObserveDiscarded(source.Task));
            JsEventLoop.Run(() =>
            {
                PromiseRuntime.ObserveDiscarded(source.Task);
                source.SetException(new InvalidOperationException("new explicit observation"));
            });
            Assert.Equal(1, Environment.ExitCode);
            Assert.Equal("new explicit observation" + Environment.NewLine, error.ToString());
        }
        finally { Console.SetError(previousError); Environment.ExitCode = previousExit; }
    }
}
