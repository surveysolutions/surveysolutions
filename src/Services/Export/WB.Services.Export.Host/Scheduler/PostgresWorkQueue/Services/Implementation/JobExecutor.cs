using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WB.Services.Export.Host.Scheduler.PostgresWorkQueue.Model;

namespace WB.Services.Export.Host.Scheduler.PostgresWorkQueue.Services.Implementation
{
    class JobExecutor : IJobExecutor
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IJobService jobService;
        private readonly ILogger<JobExecutor> logger;

        public JobExecutor(IServiceProvider serviceProvider, IJobService jobService, ILogger<JobExecutor> logger)
        {
            this.serviceProvider = serviceProvider;
            this.jobService = jobService;
            this.logger = logger;
        }

        public async Task ExecuteAsync<TService, TArg>(JobItem job, Func<TService, TArg, CancellationToken, Task> executor, CancellationToken token)
        {
            logger.LogInformation($"Executing job: [{job.Type.ToString()}] {job.Tenant} {job.Args}");

            var exportJob = serviceProvider.GetService<TService>();
            var args = JsonConvert.DeserializeObject<TArg>(job.Args);

            try
            {
                await jobService.StartJobAsync(job.Id);
                await jobService.LockJob(job.Id);
                await executor(exportJob, args, token);
                await jobService.CompleteJob(job.Id);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Error during job run: [{job.Type.ToString()}] {job.Tenant} {job.Args}");
                await jobService.FailJob(job.Id, e);
            }
            finally
            {
                await jobService.UnlockJob(job.Id);
            }
        }
    }
}
