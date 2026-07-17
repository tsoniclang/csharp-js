using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Js
{
    public delegate void PromiseResolve(object? value = null);

    public delegate void PromiseResolve<T>(T value);

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

    public static class PromiseRuntime
    {
        public static Task Create(PromiseExecutor executor)
        {
            ArgumentNullException.ThrowIfNull(executor);

            var completion = new TaskCompletionSource(
                TaskCreationOptions.RunContinuationsAsynchronously);

            void Resolve(object? _ = null) => completion.TrySetResult();
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

            void Resolve(T value) => completion.TrySetResult(value);
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

        public static Task<T[]> All(Task<T>[] values)
        {
            ArgumentNullException.ThrowIfNull(values);

            var tasks = (Task<T>[])values.Clone();
            var completion = new TaskCompletionSource<T[]>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            if (tasks.Length == 0)
            {
                completion.TrySetResult(System.Array.Empty<T>());
                return completion.Task;
            }

            var results = new T[tasks.Length];
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
    }
}
