using System;
using System.Threading;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.Enumerator.Utils
{
    public static class TaskUtils
    {
        public static Task AsTask(this CancellationTokenSource source)
        {
            if (source == null) return Task.CompletedTask;

            var completionSource = new TaskCompletionSource<object>(); //New code
            source.Token.Register(() => completionSource.TrySetCanceled()); //New code
            return completionSource.Task;
        }

        public static async Task<T> AsCancellableTask<T>(this Task<T> task, CancellationTokenSource cts)
        {
            var resultTask = await Task.WhenAny(task, cts.AsTask());

            if (resultTask != task)
            {
                cts.Token.ThrowIfCancellationRequested();
            }

            return await task;
        }

        public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, TimeSpan timeout)
        {

            using (var timeoutCancellationTokenSource = new CancellationTokenSource())
            {

                var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));
                if (completedTask == task)
                {
                    timeoutCancellationTokenSource.Cancel();
                    return await task;  // Very important in order to propagate exceptions
                }
                else
                {
                    throw new TimeoutException("The operation has timed out.");
                }
            }
        }
    }
}
