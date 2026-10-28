using System;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace Tsonic.CSharp.Js.Tests;

internal static class AllocationMeasurement
{
    public static void Run(Action action)
    {
        ExceptionDispatchInfo? failure = null;
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (Exception exception) { failure = ExceptionDispatchInfo.Capture(exception); }
        });
        thread.Start();
        thread.Join();
        failure?.Throw();
    }
}
