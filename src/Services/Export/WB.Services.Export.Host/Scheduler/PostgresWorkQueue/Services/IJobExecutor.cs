using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.Host.Scheduler.PostgresWorkQueue.Model;

namespace WB.Services.Export.Host.Scheduler.PostgresWorkQueue.Services
{
    interface IJobExecutor
    {
        Task ExecuteAsync<TService, TArg>(JobItem job, Func<TService, TArg, CancellationToken, Task> executor, CancellationToken token);
    }
}
