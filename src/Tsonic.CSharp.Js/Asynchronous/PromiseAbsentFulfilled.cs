using System;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Js;

public static partial class PromiseRuntime
{
    public static Task ThenIdentityAsync(
        Task source, Func<Task>? fulfilled = null, Func<object?, Task>? rejected = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        return CompleteThenIdentityAsync(DeferredTaskAwaiter.Create(source), fulfilled, rejected);
    }

    private static async Task CompleteThenIdentityAsync(
        DeferredTaskAwaiter deferred, Func<Task>? fulfilled, Func<object?, Task>? rejected)
    {
        try
        {
            await deferred;
        }
        catch (Exception exception) when (JsEventLoop.IsRunning)
        {
            if (rejected is null) throw;
            await rejected(RejectionReason(exception));
            return;
        }
        if (fulfilled is not null) await fulfilled();
    }

    public static Task<TStorage> ThenOptional<TResult, TStorage, TOperations>(
        Task source, Func<TResult>? fulfilled, Func<object?, TResult> rejected)
        where TOperations : struct, IOptionalStorage<TResult, TStorage>
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(rejected);
        return CompleteThenOptional<TResult, TStorage, TOperations>(
            DeferredTaskAwaiter.Create(source), fulfilled, rejected);
    }

    private static async Task<TStorage> CompleteThenOptional<TResult, TStorage, TOperations>(
        DeferredTaskAwaiter deferred, Func<TResult>? fulfilled, Func<object?, TResult> rejected)
        where TOperations : struct, IOptionalStorage<TResult, TStorage>
    {
        try
        {
            await deferred;
        }
        catch (Exception exception) when (JsEventLoop.IsRunning)
        {
            return TOperations.From2(rejected(RejectionReason(exception)));
        }
        return fulfilled is null ? TOperations.From1(null) : TOperations.From2(fulfilled());
    }

    public static Task<TStorage> ThenOptionalAsync<TResult, TStorage, TOperations>(
        Task source, Func<Task<TResult>>? fulfilled, Func<object?, Task<TResult>> rejected)
        where TOperations : struct, IOptionalStorage<TResult, TStorage>
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(rejected);
        return CompleteThenOptionalAsync<TResult, TStorage, TOperations>(
            DeferredTaskAwaiter.Create(source), fulfilled, rejected);
    }

    private static async Task<TStorage> CompleteThenOptionalAsync<TResult, TStorage, TOperations>(
        DeferredTaskAwaiter deferred, Func<Task<TResult>>? fulfilled, Func<object?, Task<TResult>> rejected)
        where TOperations : struct, IOptionalStorage<TResult, TStorage>
    {
        try
        {
            await deferred;
        }
        catch (Exception exception) when (JsEventLoop.IsRunning)
        {
            return TOperations.From2(await rejected(RejectionReason(exception)));
        }
        return fulfilled is null ? TOperations.From1(null) : TOperations.From2(await fulfilled());
    }
}

public static partial class PromiseRuntime<T>
{
    public static Task<T> ThenIdentity(
        Task<T> source, Func<T, T>? fulfilled = null, Func<object?, T>? rejected = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        return CompleteThenIdentity(DeferredTaskAwaiter<T>.Create(source), fulfilled, rejected);
    }

    private static async Task<T> CompleteThenIdentity(
        DeferredTaskAwaiter<T> deferred, Func<T, T>? fulfilled, Func<object?, T>? rejected)
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
        return fulfilled is null ? value : fulfilled(value);
    }

    public static Task<T> ThenIdentityAsync(
        Task<T> source, Func<T, Task<T>>? fulfilled, Func<object?, Task<T>> rejected)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(rejected);
        return CompleteThenIdentityAsync(DeferredTaskAwaiter<T>.Create(source), fulfilled, rejected);
    }

    private static async Task<T> CompleteThenIdentityAsync(
        DeferredTaskAwaiter<T> deferred, Func<T, Task<T>>? fulfilled, Func<object?, Task<T>> rejected)
    {
        T value;
        try
        {
            value = await deferred;
        }
        catch (Exception exception) when (JsEventLoop.IsRunning)
        {
            return await rejected(PromiseRuntime.RejectionReason(exception));
        }
        return fulfilled is null ? value : await fulfilled(value);
    }

    public static Task<TStorage> ThenOptional<TStorage, TOperations>(
        Task<T> source, Action<T>? fulfilled, Action<object?> rejected)
        where TOperations : struct, IOptionalStorage<T, TStorage>
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(rejected);
        return CompleteThenOptional<TStorage, TOperations>(
            DeferredTaskAwaiter<T>.Create(source), fulfilled, rejected);
    }

    private static async Task<TStorage> CompleteThenOptional<TStorage, TOperations>(
        DeferredTaskAwaiter<T> deferred, Action<T>? fulfilled, Action<object?> rejected)
        where TOperations : struct, IOptionalStorage<T, TStorage>
    {
        T value;
        try
        {
            value = await deferred;
        }
        catch (Exception exception) when (JsEventLoop.IsRunning)
        {
            rejected(PromiseRuntime.RejectionReason(exception));
            return TOperations.From1(null);
        }
        if (fulfilled is null) return TOperations.From2(value);
        fulfilled(value);
        return TOperations.From1(null);
    }

    public static Task<TStorage> ThenOptionalAsync<TStorage, TOperations>(
        Task<T> source, Func<T, Task>? fulfilled, Func<object?, Task> rejected)
        where TOperations : struct, IOptionalStorage<T, TStorage>
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(rejected);
        return CompleteThenOptionalAsync<TStorage, TOperations>(
            DeferredTaskAwaiter<T>.Create(source), fulfilled, rejected);
    }

    private static async Task<TStorage> CompleteThenOptionalAsync<TStorage, TOperations>(
        DeferredTaskAwaiter<T> deferred, Func<T, Task>? fulfilled, Func<object?, Task> rejected)
        where TOperations : struct, IOptionalStorage<T, TStorage>
    {
        T value;
        try
        {
            value = await deferred;
        }
        catch (Exception exception) when (JsEventLoop.IsRunning)
        {
            await rejected(PromiseRuntime.RejectionReason(exception));
            return TOperations.From1(null);
        }
        if (fulfilled is null) return TOperations.From2(value);
        await fulfilled(value);
        return TOperations.From1(null);
    }
}
