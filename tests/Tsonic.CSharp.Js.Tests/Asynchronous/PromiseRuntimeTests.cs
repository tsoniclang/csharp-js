using System;
using System.Threading.Tasks;
using Tsonic.CSharp.Runtime;
using Xunit;

namespace Tsonic.CSharp.Js.Tests
{
    public class PromiseRuntimeTests
    {
        [Fact]
        public async Task Resolved_KeepsNullValueDistinctFromTaskAdoption()
        {
            var value = PromiseRuntime<object?>.Resolved(null);
            Assert.Null(await value);
            Assert.Same(value, PromiseRuntime<object?>.Resolve(value));
            Assert.Throws<ArgumentNullException>(() => { _ = PromiseRuntime<object?>.Resolve(null!); });
        }

        [Fact]
        public async Task Resolved_PreservesNativeIntegerWidth()
        {
            const ulong value = 9007199254740993UL;
            Assert.Equal(value, await PromiseRuntime<ulong>.Resolved(value));
        }

        [Fact]
        public async Task NativeCompletion_UsesExistingJsContinuationContract()
        {
            var task = TaskCompletion<int>.Create((resolve, _) => resolve(42));
            Assert.Equal(43, await PromiseRuntime<int>.Then(task, value => value + 1));
        }

        [Fact]
        public async Task NativeCompletion_ProjectsCanonicalClosedRejectionReason()
        {
            var reason = new Reason();
            var task = TaskCompletion<object>.Create((_, reject) => reject(TsValue.from(reason)));
            Assert.Same(reason, await PromiseRuntime<object>.Catch(task, value => value!));
        }

        [Fact]
        public async Task Reject_PreservesOriginalExceptionIdentity()
        {
            var reason = new InvalidOperationException("original");
            var task = PromiseRuntime<object>.Reject(reason);
            Assert.Same(reason, await Assert.ThrowsAsync<InvalidOperationException>(async () => await task));
            Assert.Same(reason, await PromiseRuntime<object>.Catch(task, value => value!));
        }

        [Fact]
        public async Task Reject_UsesCanonicalThrownValueException()
        {
            var task = PromiseRuntime<int>.Reject("reason");
            var exception = await Assert.ThrowsAsync<TsThrownValueException>(async () => await task);
            Assert.Equal("reason", TsValue.UnwrapClosedValue(exception.value));
            Assert.Equal("reason", await PromiseRuntime<int>.Then(task, _ => "unreachable", value => (string)value!));
        }

        [Fact]
        public async Task Reject_ClosedValueAndAbsenceSurviveVoidContinuations()
        {
            var reason = new Reason();
            object? observed = null;
            await PromiseRuntime.Catch(PromiseRuntime.Reject(TsValue.from(reason)), value => observed = value);
            Assert.Same(reason, observed);
            observed = reason;
            await PromiseRuntime.Catch(PromiseRuntime.Reject(), value => observed = value);
            Assert.Null(observed);
        }

        [Fact]
        public async Task AllSettled_ProjectsCanonicalReasonForVoidAndTypedTasks()
        {
            var reason = new Reason();
            var untyped = await PromiseRuntime.AllSettled(new[] { PromiseRuntime.Reject(reason) });
            var typed = await PromiseRuntime<int>.AllSettled(new[] { PromiseRuntime<int>.Reject(reason) });
            Assert.Same(reason, untyped[0].As2().reason);
            Assert.Same(reason, typed[0].As2().reason);
        }

        [Fact]
        public void Reject_RejectsUnclosedNativeObjectRatherThanAddingReflection()
        {
            Assert.Throws<NotSupportedException>(() => { _ = PromiseRuntime<int>.Reject(new object()); });
        }

        [Fact]
        public async Task All_PreservesInputOrder()
        {
            var first = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            var second = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            var combined = PromiseRuntime<int>.All(
                new JSArray<Task<int>>(new[] { first.Task, second.Task }));

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
            Assert.Empty(await PromiseRuntime<int>.All(new JSArray<Task<int>>()));
        }

        [Fact]
        public async Task All_RejectsWithoutWaitingForRemainingTasks()
        {
            var pending = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            var expected = new InvalidOperationException("first rejection");
            var rejected = Task.FromException<int>(expected);
            var combined = PromiseRuntime<int>.All(
                new JSArray<Task<int>>(new[] { pending.Task, rejected }));

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await combined.WaitAsync(TimeSpan.FromSeconds(1)));

            Assert.Same(expected, actual);
            Assert.False(pending.Task.IsCompleted);
        }

        [Fact]
        public async Task All_RejectsNullTaskCarrier()
        {
            var combined = PromiseRuntime<int>.All(
                new JSArray<Task<int>>(new Task<int>[] { null! }));

            await Assert.ThrowsAsync<TypeError>(async () => await combined);
        }

        [Fact]
        public async Task All_MapsCancellationToRejection()
        {
            var canceled = Task.FromCanceled<int>(new System.Threading.CancellationToken(true));
            var combined = PromiseRuntime<int>.All(
                new JSArray<Task<int>>(new[] { canceled }));

            await Assert.ThrowsAsync<TaskCanceledException>(async () => await combined);
        }

        private sealed class Reason : ITsClosedValueCarrier {}
    }
}
