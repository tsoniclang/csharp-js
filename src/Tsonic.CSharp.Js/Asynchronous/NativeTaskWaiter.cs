using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Js;

internal abstract class NativeTaskWaiter(LoopSynchronizationContext context, int identity)
{
    internal static readonly SendOrPostCallback InvokeCallback = static state => ((NativeTaskWaiter)state!).Dispatch();
    internal int Identity => identity;
    internal void Complete() => context.Complete(this);
    private void Dispatch() => context.Invoke(this);
    internal abstract void Invoke();
    internal abstract void Cancel();
}

internal sealed class ReactionTaskWaiter(LoopSynchronizationContext context, int identity, Action continuation)
    : NativeTaskWaiter(context, identity)
{
    private Action? _continuation = continuation;
    internal override void Invoke()
    {
        var callback = _continuation;
        _continuation = null;
        callback?.Invoke();
    }
    internal override void Cancel() => Invoke();
}

internal sealed class DiscardedTaskWaiter(LoopSynchronizationContext context, int identity, Task source)
    : NativeTaskWaiter(context, identity)
{
    private Task? _source = source;
    internal override void Invoke()
    {
        var pending = _source;
        _source = null;
        if (pending is { IsCompletedSuccessfully: false }) PromiseRuntime.ReportDiscarded(pending);
    }
    internal override void Cancel() => _source = null;
}
