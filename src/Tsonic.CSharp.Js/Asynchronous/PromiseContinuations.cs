using System;
using System.Threading.Tasks;
using Tsonic.CSharp.Runtime;

namespace Tsonic.CSharp.Js;

public static partial class PromiseRuntime
{
    public static Task Then(Task source, Action? fulfilled = null, Action<object?>? rejected = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        return CompleteThen(DeferredTaskAwaiter.Create(source), fulfilled, rejected);
    }

    private static async Task CompleteThen(
        DeferredTaskAwaiter deferred, Action? fulfilled, Action<object?>? rejected)
    {
        try
        {
            await deferred;
        }
        catch (Exception exception) when (JsEventLoop.IsRunning)
        {
            if (rejected is null) throw;
            rejected(RejectionReason(exception));
            return;
        }
        fulfilled?.Invoke();
    }

    public static Task<TResult> Then<TResult>(
        Task source, Func<TResult> fulfilled, Func<object?, TResult>? rejected = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(fulfilled);
        return CompleteThen(DeferredTaskAwaiter.Create(source), fulfilled, rejected);
    }

    private static async Task<TResult> CompleteThen<TResult>(
        DeferredTaskAwaiter deferred, Func<TResult> fulfilled, Func<object?, TResult>? rejected)
    {
        try
        {
            await deferred;
        }
        catch (Exception exception) when (JsEventLoop.IsRunning)
        {
            if (rejected is null) throw;
            return rejected(RejectionReason(exception));
        }
        return fulfilled();
    }

    public static Task<TResult> ThenAsync<TResult>(
        Task source, Func<Task<TResult>> fulfilled, Func<object?, Task<TResult>>? rejected = null) =>
        Then<Task<TResult>>(source, fulfilled, rejected).Unwrap();

    public static Task Catch(Task source, Action<object?> rejected)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(rejected);
        return CompleteCatch(DeferredTaskAwaiter.Create(source), rejected);
    }

    private static async Task CompleteCatch(DeferredTaskAwaiter deferred, Action<object?> rejected)
    {
        try
        {
            await deferred;
        }
        catch (Exception exception) when (JsEventLoop.IsRunning)
        {
            rejected(RejectionReason(exception));
        }
    }

    internal static object? RejectionReason(Exception exception) =>
        exception is TsThrownValueException thrown ? thrown.value : exception;
}

public static partial class PromiseRuntime<T>
{
    public static Task<TResult> Then<TResult>(
        Task<T> source, Func<T, TResult> fulfilled, Func<object?, TResult>? rejected = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(fulfilled);
        return CompleteThen(DeferredTaskAwaiter<T>.Create(source), fulfilled, rejected);
    }

    private static async Task<TResult> CompleteThen<TResult>(
        DeferredTaskAwaiter<T> deferred, Func<T, TResult> fulfilled, Func<object?, TResult>? rejected)
    {
        T value;
        try
        {
            value = await deferred;
        }
        catch (Exception exception) when (JsEventLoop.IsRunning)
        {
            if (rejected is null) throw;
            return rejected(PromiseRuntime.RejectionReason(exception));
        }
        return fulfilled(value);
    }

    public static Task Then(Task<T> source, Action<T> fulfilled, Action<object?>? rejected = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(fulfilled);
        return CompleteThen(DeferredTaskAwaiter<T>.Create(source), fulfilled, rejected);
    }

    private static async Task CompleteThen(
        DeferredTaskAwaiter<T> deferred, Action<T> fulfilled, Action<object?>? rejected)
    {
        T value;
        try
        {
            value = await deferred;
        }
        catch (Exception exception) when (JsEventLoop.IsRunning)
        {
            if (rejected is null) throw;
            rejected(PromiseRuntime.RejectionReason(exception));
            return;
        }
        fulfilled(value);
    }

    public static Task<TResult> ThenAsync<TResult>(
        Task<T> source, Func<T, Task<TResult>> fulfilled, Func<object?, Task<TResult>>? rejected = null) =>
        Then<Task<TResult>>(source, fulfilled, rejected).Unwrap();

    public static Task<T> Catch(Task<T> source, Func<object?, T> rejected)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(rejected);
        return CompleteCatch(DeferredTaskAwaiter<T>.Create(source), rejected);
    }

    private static async Task<T> CompleteCatch(DeferredTaskAwaiter<T> deferred, Func<object?, T> rejected)
    {
        try
        {
            return await deferred;
        }
        catch (Exception exception) when (JsEventLoop.IsRunning)
        {
            return rejected(PromiseRuntime.RejectionReason(exception));
        }
    }
}
