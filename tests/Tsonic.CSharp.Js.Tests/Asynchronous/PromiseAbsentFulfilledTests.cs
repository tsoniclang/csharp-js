using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Tsonic.CSharp.Js.Tests;

public class PromiseAbsentFulfilledTests
{
    [Fact]
    public void AbsentFulfilled_PreservesExactValuesAndReferenceIdentityAndDefers()
    {
        JsEventLoop.Run(async () =>
        {
            const ulong value = 9007199254740993UL;
            var order = new List<string>();
            var pending = PromiseRuntime<ulong>.ThenIdentity(Task.FromResult(value));
            order.Add("after");
            Assert.False(pending.IsCompleted);
            Assert.Equal(value, await pending);
            order.Add("continued");
            Assert.Equal(new[] { "after", "continued" }, order);
            var identity = new object();
            Assert.Same(identity, await PromiseRuntime<object>.ThenIdentity(Task.FromResult(identity), null, _ => new object()));
            await PromiseRuntime.Then(Task.CompletedTask);
        });
    }

    [Fact]
    public void AbsentFulfilled_PreservesRejectionIdentityAndDoesNotCatchItsHandler()
    {
        JsEventLoop.Run(async () =>
        {
            var reason = new object();
            Assert.Equal(7, await PromiseRuntime<int>.ThenIdentity(PromiseRuntime<int>.Reject(reason), null, received =>
            {
                Assert.Same(reason, received);
                return 7;
            }));
            var expected = new InvalidOperationException("rejected callback");
            var failed = PromiseRuntime<int>.ThenIdentity(PromiseRuntime<int>.Reject(reason), null, _ => throw expected);
            Assert.Same(expected, await Assert.ThrowsAsync<InvalidOperationException>(async () => await failed));
            var native = new InvalidOperationException("unhandled source");
            var unhandled = PromiseRuntime<int>.ThenIdentity(Task.FromException<int>(native));
            Assert.Same(native, await Assert.ThrowsAsync<InvalidOperationException>(async () => await unhandled));
        });
    }

    [Fact]
    public void VoidRejected_UsesRealAbsenceAndPreservesPresentZero()
    {
        JsEventLoop.Run(async () =>
        {
            var calls = 0;
            var present = PromiseRuntime<int>.ThenOptional<int?, ValueOptionalStorage<int>>(
                Task.FromResult(0), null, _ => calls++);
            Assert.Equal(0, await present);
            Assert.Equal(0, calls);
            var missing = PromiseRuntime<int>.ThenOptional<int?, ValueOptionalStorage<int>>(
                PromiseRuntime<int>.Reject("reason"), null, received =>
                {
                    Assert.Equal("reason", received);
                    calls++;
                });
            Assert.Null(await missing);
            Assert.Equal(1, calls);
            Assert.Equal(9007199254740993L, await PromiseRuntime<long>.ThenOptional<long?, ValueOptionalStorage<long>>(
                Task.FromResult(9007199254740993L), null, _ => throw new Exception("not rejected")));
        });
    }

    [Fact]
    public void VoidSource_DoesNotReplaceAbsenceWithDefaultResult()
    {
        JsEventLoop.Run(async () =>
        {
            Assert.Null(await PromiseRuntime.ThenOptional<int, int?, ValueOptionalStorage<int>>(
                Task.CompletedTask, null, _ => 7));
            Assert.Equal(0, await PromiseRuntime.ThenOptional<int, int?, ValueOptionalStorage<int>>(
                PromiseRuntime.Reject("reason"), null, _ => 0));
            var reason = new object();
            await PromiseRuntime.Then(PromiseRuntime.Reject(reason), null, received => Assert.Same(reason, received));
        });
    }

    [Fact]
    public void AsyncRejected_IsAdoptedWithoutChangingTheFulfilledCarrier()
    {
        JsEventLoop.Run(async () =>
        {
            const long value = 9007199254740993L;
            Assert.Equal(value, await PromiseRuntime<long>.ThenIdentityAsync(
                Task.FromResult(value), null, _ => Task.FromResult(0L)));
            Assert.Equal(value, await PromiseRuntime<long>.ThenIdentityAsync(
                PromiseRuntime<long>.Reject("reason"), null, _ => Task.FromResult(value)));
            Assert.Null(await PromiseRuntime<int>.ThenOptionalAsync<int?, ValueOptionalStorage<int>>(
                PromiseRuntime<int>.Reject("reason"), null, _ => Task.CompletedTask));
            Assert.Equal(0, await PromiseRuntime<int>.ThenOptionalAsync<int?, ValueOptionalStorage<int>>(
                Task.FromResult(0), null, _ => throw new Exception("not rejected")));
            Assert.Null(await PromiseRuntime.ThenOptionalAsync<int, int?, ValueOptionalStorage<int>>(
                Task.CompletedTask, null, _ => Task.FromResult(7)));
            Assert.Equal(0, await PromiseRuntime.ThenOptionalAsync<int, int?, ValueOptionalStorage<int>>(
                PromiseRuntime.Reject("reason"), null, _ => Task.FromResult(0)));
            await PromiseRuntime.ThenIdentityAsync(PromiseRuntime.Reject("reason"), null, _ => Task.CompletedTask);
        });
    }
}
