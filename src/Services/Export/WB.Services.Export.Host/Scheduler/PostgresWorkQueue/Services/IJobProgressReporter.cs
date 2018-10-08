using System;
using System.Threading;
using System.Threading.Tasks;

namespace WB.Services.Export.Host.Scheduler.PostgresWorkQueue.Services
{
    interface IJobProgressReporter : IDisposable
    {
        void Add<T>(Func<T, CancellationToken, Task> func);
        void Start();
    }
}
