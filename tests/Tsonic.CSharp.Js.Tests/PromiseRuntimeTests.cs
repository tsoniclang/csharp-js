using System;
using System.Threading.Tasks;
using Xunit;

namespace Tsonic.CSharp.Js.Tests
{
    public class PromiseRuntimeTests
    {
        [Fact]
        public async Task Create_ResolvesVoidPromise()
        {
            var task = PromiseRuntime.Create((resolve, _) => resolve());

            await task;

            Assert.True(task.IsCompletedSuccessfully);
        }

        [Fact]
        public async Task Create_ResolvesTypedPromise()
        {
            var task = PromiseRuntime<int>.Create((resolve, _) => resolve(42));

            Assert.Equal(42, await task);
        }

        [Fact]
        public async Task Create_UsesFirstSettlement()
        {
            var task = PromiseRuntime<int>.Create((resolve, reject) =>
            {
                resolve(7);
                reject(new InvalidOperationException("late rejection"));
                resolve(9);
            });

            Assert.Equal(7, await task);
        }

        [Fact]
        public async Task Create_RejectsWithExceptionReason()
        {
            var expected = new InvalidOperationException("rejected");
            var task = PromiseRuntime<int>.Create((_, reject) => reject(expected));

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(async () => await task);

            Assert.Same(expected, actual);
        }

        [Fact]
        public async Task Create_WrapsNonExceptionReason()
        {
            var task = PromiseRuntime<int>.Create((_, reject) => reject("reason"));

            var actual = await Assert.ThrowsAsync<PromiseRejectionException>(async () => await task);

            Assert.Equal("reason", actual.Reason);
        }

        [Fact]
        public async Task Create_RejectsSynchronousExecutorThrow()
        {
            var expected = new InvalidOperationException("executor failed");
            var task = PromiseRuntime<int>.Create((_, _) => throw expected);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(async () => await task);

            Assert.Same(expected, actual);
        }

        [Fact]
        public async Task Create_IgnoresThrowAfterResolution()
        {
            var task = PromiseRuntime<int>.Create((resolve, _) =>
            {
                resolve(11);
                throw new InvalidOperationException("late throw");
            });

            Assert.Equal(11, await task);
        }

        [Fact]
        public async Task All_PreservesInputOrder()
        {
            var first = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            var second = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            var combined = PromiseRuntime<int>.All([first.Task, second.Task]);

            second.SetResult(2);
            first.SetResult(1);

            Assert.Collection(
                await combined,
                value => Assert.Equal(1, value),
                value => Assert.Equal(2, value));
        }

        [Fact]
        public async Task All_ResolvesEmptyInput()
        {
            Assert.Empty(await PromiseRuntime<int>.All([]));
        }

        [Fact]
        public async Task All_RejectsWithoutWaitingForRemainingTasks()
        {
            var pending = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            var expected = new InvalidOperationException("first rejection");
            var rejected = Task.FromException<int>(expected);
            var combined = PromiseRuntime<int>.All([pending.Task, rejected]);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await combined.WaitAsync(TimeSpan.FromSeconds(1)));

            Assert.Same(expected, actual);
            Assert.False(pending.Task.IsCompleted);
        }

        [Fact]
        public async Task All_RejectsNullTaskCarrier()
        {
            var combined = PromiseRuntime<int>.All([null!]);

            await Assert.ThrowsAsync<TypeError>(async () => await combined);
        }

        [Fact]
        public async Task All_MapsCancellationToRejection()
        {
            var canceled = Task.FromCanceled<int>(new System.Threading.CancellationToken(true));
            var combined = PromiseRuntime<int>.All([canceled]);

            await Assert.ThrowsAsync<TaskCanceledException>(async () => await combined);
        }
    }
}
