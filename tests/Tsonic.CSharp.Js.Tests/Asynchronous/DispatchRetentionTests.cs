using System;
using Xunit;

namespace Tsonic.CSharp.Js.Tests;

[Collection("JavaScript event loop")]
public sealed class DispatchRetentionTests
{
    [Fact]
    public void FailedCallbackPreservesOriginalExceptionAndRetiresUninvokedWork()
    {
        var original = new InvalidOperationException("original queued failure");
        var observed = false;
        LoopDispatch old = default;
        var returned = Assert.Throws<InvalidOperationException>(() => JsEventLoop.Run(() =>
        {
            old = JsEventLoop.Capture();
            JsEventLoop.Enqueue(() => throw original);
            JsEventLoop.Enqueue(() => observed = true);
        }));
        Assert.Same(original, returned);
        Assert.False(observed);
        Assert.False(ProcessKeepAlive.HasReferences);
        JsEventLoop.Run(() =>
        {
            Assert.False(old.IsActive);
            Assert.False(old.TryEnqueue(() => observed = true));
        });
        Assert.False(observed);
        Assert.False(ProcessKeepAlive.HasReferences);
    }

    [Fact]
    public void ReentrantAdmissionRetainsExactOrderAndReleasesReferences()
    {
        var observed = 0;
        JsEventLoop.Run(() =>
        {
            JsEventLoop.Enqueue(() =>
            {
                Assert.Equal(0, observed);
                JsEventLoop.Enqueue(() =>
                {
                    Assert.Equal(2, observed);
                    observed = 3;
                });
                observed = 1;
            });
            JsEventLoop.Enqueue(() =>
            {
                Assert.Equal(1, observed);
                observed = 2;
            });
        });
        Assert.Equal(3, observed);
        Assert.False(ProcessKeepAlive.HasReferences);
    }

    [Fact]
    public void WarmedNativeDispatchAllocatesNoCallbackSnapshots()
    {
        JsEventLoop.Run();
        var observed = 0;
        Action callback = () => observed++;
        var allocated = ExplicitLoopHost.MeasureDispatch(() =>
        {
            JsEventLoop.Enqueue(callback);
            JsEventLoop.Enqueue(callback);
        });
        Assert.Equal(0, allocated);
        Assert.Equal(2, observed);
        Assert.False(ProcessKeepAlive.HasReferences);
    }

    [Fact]
    public void NativeReferencesRejectUnderflowAndAllocateNoKeeperState()
    {
        AllocationMeasurement.Run(() => JsEventLoop.Run(() =>
        {
            Assert.False(ProcessKeepAlive.HasReferences);
            Assert.Throws<InvalidOperationException>(ProcessKeepAlive.Release);
            ProcessKeepAlive.Acquire();
            ProcessKeepAlive.Acquire();
            ProcessKeepAlive.Release();
            Assert.True(ProcessKeepAlive.HasReferences);
            ProcessKeepAlive.Release();
            Assert.False(ProcessKeepAlive.HasReferences);
            for (var index = 0; index < 100; index++)
            {
                ProcessKeepAlive.Acquire();
                ProcessKeepAlive.Release();
            }
            var before = GC.GetAllocatedBytesForCurrentThread();
            for (var index = 0; index < 10_000; index++)
            {
                ProcessKeepAlive.Acquire();
                ProcessKeepAlive.Release();
            }
            Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - before);
        }));
    }
}
