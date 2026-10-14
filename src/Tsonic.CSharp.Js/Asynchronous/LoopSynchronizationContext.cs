using System.Threading;

namespace Tsonic.CSharp.Js;

internal sealed class LoopSynchronizationContext(LoopContext owner, long generation) : SynchronizationContext
{
    public override void Post(SendOrPostCallback callback, object? state) => owner.Post(generation, callback, state);
    public override void Send(SendOrPostCallback callback, object? state) => owner.Send(generation, callback, state);
    public override SynchronizationContext CreateCopy() => this;
    internal void Complete(NativeTaskWaiter waiter) => owner.CompleteWaiter(generation, waiter);
    internal void Invoke(NativeTaskWaiter waiter) => owner.InvokeWaiter(generation, waiter);
    internal void Notify()
    {
        owner.NotifyStateChanged(generation);
    }
}
