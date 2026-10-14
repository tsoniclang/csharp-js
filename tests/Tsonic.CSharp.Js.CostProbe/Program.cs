using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tsonic.CSharp.Js;

namespace Tsonic.CSharp.Js.CostProbe;

public static class Program
{
    private const int Count = 4096;
    private static int _called;
    private static int _fulfilled;
    private static int _rejected;
    private static readonly Action Job = static () => _called++;
    private static readonly Func<int, int> Fulfilled = static value => { _fulfilled++; return value + 1; };
    private static readonly Func<object?, int> Rejected = static _ => { _rejected++; return 0; };
    private static readonly Task<int> Source = Task.FromResult(123);
    private static readonly Task<int> RejectedSource = Task.FromException<int>(new InvalidOperationException("native rejection input"));

    private static void Main()
    {
        var native = new NativeControl();
        Validate(native.Run, NativeControl.Then);
        Validate(JsEventLoop.Run, PromiseRuntime<int>.Then<int>);
        var empty = (Action)(static () => { });
        for (var warm = 0; warm < 128; warm++) { native.Run(empty); JsEventLoop.Run(empty); }
        var coldBefore = GC.GetTotalAllocatedBytes(true);
        for (var iteration = 0; iteration < 128; iteration++) native.Run(empty);
        var nativeCold = GC.GetTotalAllocatedBytes(true) - coldBefore;
        coldBefore = GC.GetTotalAllocatedBytes(true);
        for (var iteration = 0; iteration < 128; iteration++) JsEventLoop.Run(empty);
        var actualCold = GC.GetTotalAllocatedBytes(true) - coldBefore;
        Console.WriteLine($"cold epoch activation: native={nativeCold / 128.0}, actual={actualCold / 128.0} B/activation");
        if (nativeCold != actualCold) throw new InvalidOperationException("cold native epoch allocation differs");
        Measure("jobs", native.Run, () => { for (var index = 0; index < Count; index++) native.Post(Job); },
            () => { for (var index = 0; index < Count; index++) JsEventLoop.Enqueue(Job); });
        Measure("then", native.Run, () => { for (var index = 0; index < Count; index++) _ = NativeControl.Then(Source, Fulfilled); },
            () => { for (var index = 0; index < Count; index++) _ = PromiseRuntime<int>.Then(Source, Fulfilled); });
        Measure("catch (fulfilled source)", native.Run, () => { for (var index = 0; index < Count; index++) _ = NativeControl.Catch(Source, Rejected); },
            () => { for (var index = 0; index < Count; index++) _ = PromiseRuntime<int>.Catch(Source, Rejected); });
        Measure("catch (rejected source)", native.Run, () => { for (var index = 0; index < Count; index++) _ = NativeControl.Catch(RejectedSource, Rejected); },
            () => { for (var index = 0; index < Count; index++) _ = PromiseRuntime<int>.Catch(RejectedSource, Rejected); });
        Measure("finally", native.Run, () => { for (var index = 0; index < Count; index++) _ = NativeControl.Finally(Source, Job); },
            () => { for (var index = 0; index < Count; index++) _ = PromiseRuntime<int>.Finally(Source, Job); });
        var sources = new TaskCompletionSource<int>[Count];
        foreach (var options in new[] { TaskCreationOptions.None, TaskCreationOptions.RunContinuationsAsynchronously })
        {
            void Prepare() { for (var index = 0; index < Count; index++) sources[index] = new(options); }
            Measure("pending then + completion + drain + shutdown (" + options + ")", native.Run,
                () =>
                {
                    foreach (var source in sources) _ = NativeControl.Then(source.Task, Fulfilled);
                    foreach (var source in sources) source.SetResult(123);
                },
                () =>
                {
                    foreach (var source in sources) _ = PromiseRuntime<int>.Then(source.Task, Fulfilled);
                    foreach (var source in sources) source.SetResult(123);
                }, Prepare);
        }
        void CheckNativeAdmission(Action<Action> run, bool subject)
        {
            for (var iteration = 0; iteration < 256; iteration++) run(() =>
            {
                var source = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
                if (subject) _ = PromiseRuntime<int>.Then(source.Task, value => { _called++; return value; });
                else _ = NativeControl.Then(source.Task, value => { _called++; return value; });
                source.SetResult(1);
            });
        }
        var before = _called;
        CheckNativeAdmission(native.Run, false);
        CheckNativeAdmission(JsEventLoop.Run, true);
        if (_called - before != 512) throw new InvalidOperationException("native completion admission lost work");
        Console.WriteLine("native completion admission=512/512; all measurements use GC.GetTotalAllocatedBytes(true), separate process, zero background measurement work");
    }

    private static void Validate(Action<Action> run, Func<Task<int>, Func<int, int>, Func<object?, int>?, Task<int>> then)
    {
        var seen = new List<int>();
        var exactReason = new object();
        var failure = Tsonic.CSharp.Runtime.TsThrownValueException.from(exactReason);
        run(() =>
        {
            var source = new TaskCompletionSource<int>();
            _ = then(source.Task, value => { seen.Add(value); return value; }, null);
            _ = then(Task.FromException<int>(failure), static _ => 0, reason =>
            {
                if (!ReferenceEquals(reason, exactReason)) throw new InvalidOperationException("rejection identity changed");
                seen.Add(2);
                return 2;
            });
            source.SetResult(1);
            if (seen.Count != 0) throw new InvalidOperationException("reaction was not deferred");
        });
        if (seen.Count != 2 || seen[0] != 2 || seen[1] != 1) throw new InvalidOperationException("admission FIFO changed");
        var original = new InvalidOperationException("callback");
        var rejected = false;
        Task<int>? result = null;
        run(() => { result = then(Source, _ => throw original, _ => { rejected = true; return 0; }); });
        try { result!.GetAwaiter().GetResult(); throw new InvalidOperationException("callback failure was lost"); }
        catch (InvalidOperationException actual) when (ReferenceEquals(actual, original)) { }
        if (rejected) throw new InvalidOperationException("callback was caught by its own rejection branch");
        var unresolved = new TaskCompletionSource<int>();
        var called = false;
        run(() => { result = then(unresolved.Task, _ => { called = true; return 1; }, null); });
        if (!result!.IsCanceled) throw new InvalidOperationException("pending owner state was not canceled");
        run(() => unresolved.SetResult(1));
        if (called) throw new InvalidOperationException("old native notification entered a new owner activation");
    }

    private static void Measure(string name, Action<Action> runNative, Action control, Action subject, Action? prepare = null)
    {
        for (var warm = 0; warm < 8; warm++) { prepare?.Invoke(); runNative(control); prepare?.Invoke(); JsEventLoop.Run(subject); }
        prepare?.Invoke();
        var nativeStart = (_called, _fulfilled, _rejected);
        var before = 0L;
        runNative(() => { before = GC.GetTotalAllocatedBytes(true); control(); });
        var nativeBytes = GC.GetTotalAllocatedBytes(true) - before;
        var nativeEffects = (_called - nativeStart._called, _fulfilled - nativeStart._fulfilled, _rejected - nativeStart._rejected);
        prepare?.Invoke();
        var actualStart = (_called, _fulfilled, _rejected);
        JsEventLoop.Run(() => { before = GC.GetTotalAllocatedBytes(true); subject(); });
        var actualBytes = GC.GetTotalAllocatedBytes(true) - before;
        var actualEffects = (_called - actualStart._called, _fulfilled - actualStart._fulfilled, _rejected - actualStart._rejected);
        Console.WriteLine($"{name}: native={nativeBytes / (double)Count}, actual={actualBytes / (double)Count} B/op; total={nativeBytes}/{actualBytes}, complete-operations={Count}; native/actual callbacks={nativeEffects}/{actualEffects}");
        if (actualEffects != nativeEffects) throw new InvalidOperationException($"{name} callbacks differ from native equivalent");
        if (actualBytes != nativeBytes) throw new InvalidOperationException($"{name} allocation differs from native equivalent");
    }
}
