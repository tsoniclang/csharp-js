using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tsonic.CSharp.Runtime;

namespace Tsonic.CSharp.Js
{
    public sealed class PromiseFulfilledResult
    {
        public string status => "fulfilled";
    }

    public sealed class PromiseFulfilledResult<T>
    {
        public string status => "fulfilled";
        public T value { get; }

        public PromiseFulfilledResult(T value)
        {
            this.value = value;
        }
    }

    public sealed class PromiseRejectedResult
    {
        public string status => "rejected";
        public object? reason { get; }

        public PromiseRejectedResult(object? reason)
        {
            this.reason = reason;
        }
    }

    public static partial class PromiseRuntime
    {
        public static Task Resolve() => Task.CompletedTask;

        public static Task Resolve(Task value)
        {
            ArgumentNullException.ThrowIfNull(value);
            return value;
        }

        public static Task Reject(object? reason = null) => Task.FromException(ToException(reason));

        public static async Task Race(IEnumerable<Task> values)
        {
            ArgumentNullException.ThrowIfNull(values);
            var tasks = values.ToArray();
            if (tasks.Length == 0)
            {
                await new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously).Task.ConfigureAwait(false);
                return;
            }
            await (await Task.WhenAny(tasks).ConfigureAwait(false)).ConfigureAwait(false);
        }

        public static async Task Any(IEnumerable<Task> values)
        {
            ArgumentNullException.ThrowIfNull(values);
            var remaining = values.ToList();
            var failures = new List<Exception>();
            if (remaining.Count == 0)
            {
                throw new AggregateException("All promises were rejected.", failures);
            }
            while (remaining.Count > 0)
            {
                var completed = await Task.WhenAny(remaining).ConfigureAwait(false);
                remaining.Remove(completed);
                try
                {
                    await completed.ConfigureAwait(false);
                    return;
                }
                catch (Exception exception)
                {
                    failures.Add(exception);
                }
            }
            throw new AggregateException("All promises were rejected.", failures);
        }

        public static async Task<JSArray<Union<PromiseFulfilledResult, PromiseRejectedResult>>> AllSettled(
            IEnumerable<Task> values)
        {
            ArgumentNullException.ThrowIfNull(values);
            var results = new JSArray<Union<PromiseFulfilledResult, PromiseRejectedResult>>();
            foreach (var task in values)
            {
                try
                {
                    await task.ConfigureAwait(false);
                    results.push(new PromiseFulfilledResult());
                }
                catch (Exception exception)
                {
                    results.push(new PromiseRejectedResult(RejectionReason(exception)));
                }
            }
            return results;
        }

        public static Task Finally(Task source, Action? onFinally = null)
        {
            ArgumentNullException.ThrowIfNull(source);
            return CompleteFinally(source, onFinally);
        }

        private static async Task CompleteFinally(Task source, Action? onFinally)
        {
            Exception? failure = null;
            try
            {
                await source.ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                failure = exception;
            }

            onFinally?.Invoke();
            if (failure is not null)
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw();
            }
        }

        internal static Exception ToException(object? reason)
        {
            return TsThrownValueException.from(reason);
        }
    }

    public static partial class PromiseRuntime<T>
    {
        public static Task<T> Resolved(T value) => Task.FromResult(value);

        public static Task<T> Resolve(Task<T> value)
        {
            ArgumentNullException.ThrowIfNull(value);
            return value;
        }

        public static Task<T> Reject(object? reason = null) => Task.FromException<T>(PromiseRuntime.ToException(reason));

        public static Task<JSArray<T>> All(JSArray<Task<T>> values)
        {
            ArgumentNullException.ThrowIfNull(values);

            var tasks = new Task<T>[values.length];
            for (var index = 0; index < values.length; index++)
            {
                tasks[index] = values[index];
            }
            var completion = new TaskCompletionSource<JSArray<T>>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            if (tasks.Length == 0)
            {
                completion.TrySetResult(new JSArray<T>());
                return completion.Task;
            }

            var results = new JSArray<T>(tasks.Length);
            var remaining = tasks.Length;
            for (var index = 0; index < tasks.Length; index++)
            {
                var task = tasks[index];
                if (task is null)
                {
                    completion.TrySetException(new TypeError("Promise.all received a null Task carrier."));
                    continue;
                }

                var resultIndex = index;
                task.ContinueWith(
                    completed =>
                    {
                        if (completed.IsCompletedSuccessfully)
                        {
                            results[resultIndex] = completed.Result;
                            if (Interlocked.Decrement(ref remaining) == 0)
                            {
                                completion.TrySetResult(results);
                            }
                            return;
                        }

                        if (completed.IsCanceled)
                        {
                            completion.TrySetException(new TaskCanceledException(completed));
                            return;
                        }

                        var aggregate = completed.Exception;
                        Exception exception = aggregate is null
                            ? new InvalidOperationException("Promise.all observed a faulted Task without an exception.")
                            : aggregate.InnerExceptions.Count == 1
                                ? aggregate.InnerExceptions[0]
                                : aggregate;
                        completion.TrySetException(exception);
                    },
                    CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default);
            }

            return completion.Task;
        }

        public static async Task<T> Race(IEnumerable<Task<T>> values)
        {
            ArgumentNullException.ThrowIfNull(values);
            var tasks = values.ToArray();
            if (tasks.Length == 0)
            {
                return await new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously).Task.ConfigureAwait(false);
            }
            return await (await Task.WhenAny(tasks).ConfigureAwait(false)).ConfigureAwait(false);
        }

        public static async Task<T> Any(IEnumerable<Task<T>> values)
        {
            ArgumentNullException.ThrowIfNull(values);
            var remaining = values.ToList();
            var failures = new List<Exception>();
            if (remaining.Count == 0)
            {
                throw new AggregateException("All promises were rejected.", failures);
            }

            while (remaining.Count > 0)
            {
                var completed = await Task.WhenAny(remaining).ConfigureAwait(false);
                remaining.Remove(completed);
                try
                {
                    return await completed.ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    failures.Add(exception);
                }
            }
            throw new AggregateException("All promises were rejected.", failures);
        }

        public static async Task<JSArray<Union<PromiseFulfilledResult<T>, PromiseRejectedResult>>> AllSettled(
            IEnumerable<Task<T>> values)
        {
            ArgumentNullException.ThrowIfNull(values);
            var results = new JSArray<Union<PromiseFulfilledResult<T>, PromiseRejectedResult>>();
            foreach (var task in values)
            {
                try
                {
                    results.push(new PromiseFulfilledResult<T>(await task.ConfigureAwait(false)));
                }
                catch (Exception exception)
                {
                    results.push(new PromiseRejectedResult(PromiseRuntime.RejectionReason(exception)));
                }
            }
            return results;
        }

        public static Task<T> Finally(Task<T> source, Action? onFinally = null)
        {
            ArgumentNullException.ThrowIfNull(source);
            return CompleteFinally(source, onFinally);
        }

        private static async Task<T> CompleteFinally(Task<T> source, Action? onFinally)
        {
            Exception? failure = null;
            T? value = default;
            try
            {
                value = await source.ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                failure = exception;
            }

            onFinally?.Invoke();
            if (failure is not null)
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw();
            }
            return value!;
        }
    }
}
