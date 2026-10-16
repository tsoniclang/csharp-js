using System;

namespace Tsonic.CSharp.Js.Tests;

internal static class ExplicitLoopHost
{
    public static long MeasureDispatch(Action initialize)
    {
        long before = 0;
        JsEventLoop.Run(() =>
        {
            initialize();
            before = GC.GetAllocatedBytesForCurrentThread();
        });
        return GC.GetAllocatedBytesForCurrentThread() - before;
    }
}
