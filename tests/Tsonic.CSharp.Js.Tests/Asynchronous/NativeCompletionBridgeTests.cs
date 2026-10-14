using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tsonic.CSharp.Js.Tests;

[Collection("JavaScript event loop")]
public sealed class NativeCompletionBridgeTests
{
    [Theory]
    [InlineData(TaskCreationOptions.None, false)]
    [InlineData(TaskCreationOptions.None, true)]
    [InlineData(TaskCreationOptions.RunContinuationsAsynchronously, false)]
    [InlineData(TaskCreationOptions.RunContinuationsAsynchronously, true)]
    public void NativeTaskAdmissionIsSynchronousWithCompletionWithoutExecutingADeferredUserJob(
        TaskCreationOptions options, bool foreign)
    {
        var context = new NativeContext();
        var previous = SynchronizationContext.Current;
        var callbacks = 0;
        var source = new TaskCompletionSource(options);
        var postsBeforeProducerReturned = -1;
        Exception? failure = null;
        try
        {
            SynchronizationContext.SetSynchronizationContext(context);
            source.Task.GetAwaiter().UnsafeOnCompleted(() => callbacks++);
            void Complete()
            {
                try { source.SetResult(); postsBeforeProducerReturned = context.Posts; }
                catch (Exception error) { failure = error; }
            }
            if (foreign)
            {
                var producer = new Thread(Complete);
                producer.Start();
                Assert.True(producer.Join(TimeSpan.FromSeconds(5)));
            }
            else Complete();
            Assert.Null(failure);
            var deferred = foreign || options == TaskCreationOptions.RunContinuationsAsynchronously;
            Assert.Equal(deferred ? 1 : 0, postsBeforeProducerReturned);
            Assert.Equal(deferred ? 0 : 1, callbacks);
            context.Drain();
            Assert.Equal(1, callbacks);
        }
        finally { SynchronizationContext.SetSynchronizationContext(previous); }
    }

    [Theory]
    [InlineData(TaskCreationOptions.None, false)]
    [InlineData(TaskCreationOptions.None, true)]
    [InlineData(TaskCreationOptions.RunContinuationsAsynchronously, false)]
    [InlineData(TaskCreationOptions.RunContinuationsAsynchronously, true)]
    public void NativeCompletionAdmitsOneDeferredReactionWithNativeForeignIngressOrder(
        TaskCreationOptions options, bool foreign)
    {
        var seen = new List<string>();
        var owner = Environment.CurrentManagedThreadId;
        var source = new TaskCompletionSource<int>(options);
        JsEventLoop.Run(() =>
        {
            _ = PromiseRuntime<int>.Then(source.Task, value =>
            {
                Assert.Equal(owner, Environment.CurrentManagedThreadId);
                seen.Add("reaction");
                return value;
            });
            if (foreign)
            {
                var producer = new Thread(() => source.SetResult(1));
                producer.Start();
                Assert.True(producer.Join(TimeSpan.FromSeconds(5)));
            }
            else source.SetResult(1);
            Assert.Empty(seen);
            JsEventLoop.Enqueue(() => seen.Add("frontier"));
        });
        Assert.Equal(foreign ? new[] { "frontier", "reaction" } : new[] { "reaction", "frontier" }, seen);
    }

    [Fact]
    public void RegisteringNativeWaitersRestoresTheExactAmbientContextAndRetiresTheirNotifications()
    {
        var source = new TaskCompletionSource<int>();
        Task<int>? reaction = null;
        SynchronizationContext? captured = null;
        var called = false;
        JsEventLoop.Run(() =>
        {
            captured = SynchronizationContext.Current;
            reaction = PromiseRuntime<int>.Then(source.Task, value => { called = true; return value; });
            Assert.Same(captured, SynchronizationContext.Current);
        });
        Assert.True(reaction!.IsCanceled);
        JsEventLoop.Run(() =>
        {
            Assert.NotSame(captured, SynchronizationContext.Current);
            source.SetResult(17);
        });
        Assert.False(called);
        Assert.True(reaction.IsCanceled);
    }

    private sealed class NativeContext : SynchronizationContext
    {
        private SendOrPostCallback? _callback;
        private object? _state;
        private int _posts;
        internal int Posts => Volatile.Read(ref _posts);
        public override void Post(SendOrPostCallback callback, object? state)
        {
            _callback = callback;
            _state = state;
            Interlocked.Increment(ref _posts);
        }
        internal void Drain() => _callback?.Invoke(_state);
    }
}
