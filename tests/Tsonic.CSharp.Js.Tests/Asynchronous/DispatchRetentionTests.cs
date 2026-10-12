using System;
using Xunit;

namespace Tsonic.CSharp.Js.Tests;

[Collection("JavaScript event loop")]
public sealed class DispatchRetentionTests
{
    [Fact]
    public void FailedCallbackRetainsOriginalExceptionAndUninvokedWork()
    {
        var original = new InvalidOperationException("original queued failure");
        var observed = false;
        JsEventLoop.EnqueueReferenced(() => throw original);
        JsEventLoop.EnqueueReferenced(() => observed = true);
        var returned = Assert.Throws<InvalidOperationException>(JsEventLoop.Run);
        Assert.Same(original, returned);
        Assert.False(observed);
        Assert.True(ProcessKeepAlive.HasReferences);
        JsEventLoop.Run();
        Assert.True(observed);
        Assert.False(ProcessKeepAlive.HasReferences);
    }

    [Fact]
    public void ReentrantAdmissionRetainsExactOrderAndReleasesReferences()
    {
        var observed = 0;
        JsEventLoop.EnqueueReferenced(() =>
        {
            Assert.Equal(0, observed);
            JsEventLoop.EnqueueReferenced(() =>
            {
                Assert.Equal(2, observed);
                observed = 3;
            });
            observed = 1;
        });
        JsEventLoop.EnqueueReferenced(() =>
        {
            Assert.Equal(1, observed);
            observed = 2;
        });
        JsEventLoop.Run();
        Assert.Equal(3, observed);
        Assert.False(ProcessKeepAlive.HasReferences);
    }

    [Fact]
    public void WarmedNativeDispatchAllocatesNoCallbackSnapshots()
    {
        JsEventLoop.Run();
        var observed = 0;
        Action callback = () => observed++;
        JsEventLoop.EnqueueHandleOwned(callback);
        JsEventLoop.EnqueueHandleOwned(callback);
        var before = GC.GetAllocatedBytesForCurrentThread();
        JsEventLoop.Run();
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.Equal(0, allocated);
        Assert.Equal(2, observed);
        Assert.False(ProcessKeepAlive.HasReferences);
    }

    [Fact]
    public void NativeReferencesRejectUnderflowAndAllocateNoKeeperState()
    {
        JsEventLoop.Run();
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
    }
}
