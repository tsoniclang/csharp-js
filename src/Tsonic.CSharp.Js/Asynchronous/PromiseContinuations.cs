using System;
using System.Threading.Tasks;
using Tsonic.CSharp.Runtime;

namespace Tsonic.CSharp.Js;

public static partial class PromiseRuntime
{
    public static async Task Then(Task source, Action fulfilled, Action<object?>? rejected = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(fulfilled);
        try
        {
            await source.ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
        }
        catch (Exception exception)
        {
            if (rejected is null) throw;
            rejected(RejectionReason(exception));
            return;
        }
        fulfilled();
    }

    public static async Task<TResult> Then<TResult>(
        Task source, Func<TResult> fulfilled, Func<object?, TResult>? rejected = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(fulfilled);
        try
        {
            await source.ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
        }
        catch (Exception exception)
        {
            if (rejected is null) throw;
            return rejected(RejectionReason(exception));
        }
        return fulfilled();
    }

    public static Task<TResult> ThenAsync<TResult>(
        Task source, Func<Task<TResult>> fulfilled, Func<object?, Task<TResult>>? rejected = null) =>
        Then<Task<TResult>>(source, fulfilled, rejected).Unwrap();

    public static async Task Catch(Task source, Action<object?> rejected)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(rejected);
        try
        {
            await source.ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
        }
        catch (Exception exception)
        {
            rejected(RejectionReason(exception));
        }
    }

    internal static object? RejectionReason(Exception exception) =>
        exception is TsThrownValueException thrown ? thrown.value : exception;
}

public static partial class PromiseRuntime<T>
{
    public static async Task<TResult> Then<TResult>(
        Task<T> source, Func<T, TResult> fulfilled, Func<object?, TResult>? rejected = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(fulfilled);
        T value;
        try
        {
            value = await source.ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
        }
        catch (Exception exception)
        {
            if (rejected is null) throw;
            return rejected(PromiseRuntime.RejectionReason(exception));
        }
        return fulfilled(value);
    }

    public static async Task Then(Task<T> source, Action<T> fulfilled, Action<object?>? rejected = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(fulfilled);
        T value;
        try
        {
            value = await source.ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
        }
        catch (Exception exception)
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

    public static async Task<T> Catch(Task<T> source, Func<object?, T> rejected)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(rejected);
        try
        {
            return await source.ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
        }
        catch (Exception exception)
        {
            return rejected(PromiseRuntime.RejectionReason(exception));
        }
    }
}
