using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace WB.Services.Export.Host.Scheduler.PostgresWorkQueue.Services.Implementation
{
    class JobProgressReporter : IDisposable, IJobProgressReporter
    {
        private readonly JobContext context;
        private readonly IServiceProvider serviceProvider;

        public JobProgressReporter(JobContext context, IServiceProvider serviceProvider)
        {
            this.context = context;
            this.serviceProvider = serviceProvider;
            this.cts = new CancellationTokenSource();
        }

        public void Start()
        {
            Task.Run(async () =>
            {
                foreach (var task in queue.GetConsumingEnumerable(cts.Token))
                {
                    using (var tr = await context.Database.BeginTransactionAsync(cts.Token))
                    {
                        var instance = serviceProvider.GetService(task.type);

                        await task.func(instance, cts.Token);
                        tr.Commit();
                    }
                }
            }, cts.Token);
        }

        readonly BlockingCollection<(Type type, Func<object, CancellationToken, Task> func)> queue
            = new BlockingCollection<(Type, Func<object, CancellationToken, Task>)>();

        private readonly CancellationTokenSource cts;

        public void Add<T>(Func<T, CancellationToken, Task> func)
        {
            queue.Add((typeof(T), (obj, token) =>
                    {
                        var item = (T)obj;
                        return func(item, token);
                    }
                ));
        }

        public void Dispose()
        {
            queue.CompleteAdding();

            cts.Cancel();
            cts.Dispose();
        }
    }
}
