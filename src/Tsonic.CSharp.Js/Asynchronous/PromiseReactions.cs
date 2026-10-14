using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Js;

internal readonly struct DeferredTaskAwaiter(Task source) : ICriticalNotifyCompletion
{
    internal static DeferredTaskAwaiter Create(Task source)
    {
        _ = JsEventLoop.Current;
        return new(source);
    }
    public bool IsCompleted => false;
    public DeferredTaskAwaiter GetAwaiter() => this;
    public void GetResult()
    {
        if (!JsEventLoop.IsRunning) throw new OperationCanceledException("The JavaScript event-loop owner has shut down.");
        source.GetAwaiter().GetResult();
    }
    public void OnCompleted(Action continuation) => UnsafeOnCompleted(continuation);
    public void UnsafeOnCompleted(Action continuation) => JsEventLoop.Current.Defer(source, continuation);
}

internal readonly struct DeferredTaskAwaiter<T>(Task<T> source) : ICriticalNotifyCompletion
{
    internal static DeferredTaskAwaiter<T> Create(Task<T> source)
    {
        _ = JsEventLoop.Current;
        return new(source);
    }
    public bool IsCompleted => false;
    public DeferredTaskAwaiter<T> GetAwaiter() => this;
    public T GetResult()
    {
        if (!JsEventLoop.IsRunning) throw new OperationCanceledException("The JavaScript event-loop owner has shut down.");
        return source.GetAwaiter().GetResult();
    }
    public void OnCompleted(Action continuation) => UnsafeOnCompleted(continuation);
    public void UnsafeOnCompleted(Action continuation) => JsEventLoop.Current.Defer(source, continuation);
}

public static partial class PromiseRuntime
{
    public static void ObserveDiscarded(Task source)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (source.IsCompletedSuccessfully) return;
        var dispatch = JsEventLoop.Capture();
        if (source.IsCompleted)
        {
            dispatch.Enqueue(DiscardedReporting.Callback, source);
            return;
        }
        dispatch.Owner.Observe(source, null);
    }

    internal static void ReportDiscarded(Task source)
    {
        try { source.GetAwaiter().GetResult(); }
        catch (Exception exception)
        {
            var reason = RejectionReason(exception);
            Console.Error.WriteLine(reason is Exception error ? error.Message : reason?.ToString() ?? "null");
            Environment.ExitCode = 1;
            JsEventLoop.Capture().RequestExit();
        }
    }

    private static class DiscardedReporting
    {
        internal static readonly SendOrPostCallback Callback = static state => ReportDiscarded((Task)state!);
    }
}
