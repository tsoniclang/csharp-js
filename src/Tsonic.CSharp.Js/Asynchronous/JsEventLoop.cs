using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Js;

public static class JsEventLoop
{
    internal const string MissingLoopMessage = "No JavaScript event loop is running on this thread.";
    internal const string CapacityMessage = "Pending JavaScript callback count exceeds the finite event-loop limit.";
    [ThreadStatic] private static LoopContext? _owner;
    private static readonly PhaseRegistration TimerRegistration = new();
    public static DispatchPhase TimerPhase => TimerRegistration.Phase;

    public static LoopDispatch Capture() => Current.Capture(0);
    public static LoopDispatch RegisterPhase(DispatchPhase phase, ReadOnlySpan<DispatchPhase> order) => Current.RegisterPhase(phase, order);
    internal static LoopContext Current => _owner is { IsRunning: true } owner ? owner : throw new InvalidOperationException(MissingLoopMessage);
    internal static bool HasReferences => _owner is { IsRunning: true, HasReferences: true };
    public static bool IsRunning => _owner is { IsRunning: true };
    public static void Enqueue(Action callback) => Capture().Enqueue(callback);
    public static void Run() => RunCore(null, null);
    public static void Run(Action entrypoint)
    {
        ArgumentNullException.ThrowIfNull(entrypoint);
        RunCore(entrypoint, null);
    }
    public static void Run(Func<Task> entrypoint)
    {
        ArgumentNullException.ThrowIfNull(entrypoint);
        RunCore(null, entrypoint);
    }
    internal static void Run(Action entrypoint, int maximumCallbacks, int maximumIngress, int maximumRegistrations)
    {
        ArgumentNullException.ThrowIfNull(entrypoint);
        (_owner ??= new LoopContext()).Run(entrypoint, null, maximumCallbacks, maximumIngress, maximumRegistrations);
    }
    private static void RunCore(Action? initialize, Func<Task>? start) =>
        (_owner ??= new LoopContext()).Run(initialize, start, 1 << 20, 16 * 1024, 16 * 1024);
}
