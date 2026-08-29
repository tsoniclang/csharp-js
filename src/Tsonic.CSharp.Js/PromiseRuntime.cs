using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tsonic.CSharp.Runtime;

namespace Tsonic.CSharp.Js
{
    public delegate void PromiseResolve(object? value = null);

    public delegate void PromiseResolve<T>(Union<T, Task<T>> value);

    public delegate void PromiseReject(object? reason = null);

    public delegate void PromiseExecutor(PromiseResolve resolve, PromiseReject reject);

    public delegate void PromiseExecutor<T>(PromiseResolve<T> resolve, PromiseReject reject);

    public sealed class PromiseRejectionException : Exception
    {
        public PromiseRejectionException(object? reason)
            : base("Promise rejected with a non-Exception reason.")
        {
            Reason = reason;
        }

        public object? Reason { get; }
    }

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

    public static class PromiseRuntime
    {
        public static Task Create(PromiseExecutor executor)
        {
            ArgumentNullException.ThrowIfNull(executor);

            var completion = new TaskCompletionSource(
                TaskCreationOptions.RunContinuationsAsynchronously);

            void Resolve(object? value = null)
            {
                if (value is Task task)
                {
                    _ = CompleteFromTask(task);
                    return;
                }
                completion.TrySetResult();
            }
            async Task CompleteFromTask(Task task)
            {
                try
                {
                    await task.ConfigureAwait(false);
                    completion.TrySetResult();
                }
                catch (Exception exception)
                {
                    completion.TrySetException(exception);
                }
            }
            void Reject(object? reason = null) => completion.TrySetException(ToException(reason));

            try
            {
                executor(Resolve, Reject);
            }
            catch (Exception exception)
            {
                completion.TrySetException(exception);
            }

            return completion.Task;
        }

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
                    results.push(new PromiseRejectedResult(exception));
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
            return reason as Exception ?? new PromiseRejectionException(reason);
        }
    }

    public static class PromiseRuntime<T>
    {
        public static Task<T> Create(PromiseExecutor<T> executor)
        {
            ArgumentNullException.ThrowIfNull(executor);

            var completion = new TaskCompletionSource<T>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            void Resolve(Union<T, Task<T>> value)
            {
                ArgumentNullException.ThrowIfNull(value);
                if (value.Is1())
                {
                    completion.TrySetResult(value.As1());
                    return;
                }
                var task = value.As2();
                if (task is null)
                {
                    completion.TrySetException(
                        new TypeError("Promise resolve received a null Task carrier."));
                    return;
                }
                _ = CompleteFromTask(task);
            }
            async Task CompleteFromTask(Task<T> task)
            {
                try
                {
                    completion.TrySetResult(
                        await task.ConfigureAwait(false));
                }
                catch (Exception exception)
                {
                    completion.TrySetException(exception);
                }
            }
            void Reject(object? reason = null) => completion.TrySetException(PromiseRuntime.ToException(reason));

            try
            {
                executor(Resolve, Reject);
            }
            catch (Exception exception)
            {
                completion.TrySetException(exception);
            }

            return completion.Task;
        }

        public static Task<T> Resolve(T value) => Task.FromResult(value);

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
                    results.push(new PromiseRejectedResult(exception));
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
