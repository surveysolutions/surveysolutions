using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WB.Services.Export.Jobs;
using WB.Services.Export.Services.Processing;
using WB.Services.Export.Tenant;

namespace WB.Services.Export.Host.Scheduler
{
    public class DataExportProcessesService : IDataExportProcessesService, IHostedService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<DataExportProcessesService> logger;
        readonly BlockingCollection<DataExportProcessDetails> tempQueue = new BlockingCollection<DataExportProcessDetails>();
        private DataExportProcessDetails currentProcessingJob = null;

        public DataExportProcessesService(IServiceProvider serviceProvider, ILogger<DataExportProcessesService> logger)
        {
            logger.LogInformation("Ctor");
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        public DataExportProcessDetails GetAndStartOldestUnprocessedDataExport()
        {
            throw new NotImplementedException();
        }

        public string AddDataExport(DataExportProcessDetails args)
        {
            if (tempQueue.Any(q => q.NaturalId == args.NaturalId)) return args.NaturalId;
            if (currentProcessingJob?.NaturalId == args.NaturalId) return args.NaturalId;

            if(!tempQueue.IsAddingCompleted) tempQueue.Add(args);

            return args.NaturalId;
        }
        
        
        public IEnumerable<DataExportProcessDetails> GetRunningExportProcesses(TenantInfo tenant)
        {
            return GetAllProcesses(tenant).Where(p => p.IsQueuedOrRunning());
        }

        public DataExportProcessDetails[] GetAllProcesses(TenantInfo tenant)
        {
            return tempQueue.Where(a => a.Tenant.Equals(tenant))
                .Concat(new [] { currentProcessingJob })
                .Where(p => p != null)
                .ToArray();
        }

        public void FinishExportSuccessfully(string processId)
        {

           // throw new NotImplementedException();
        }

        public void FinishExportWithError(string processId, Exception e)
        {
         //   throw new NotImplementedException();
        }

        public void UpdateDataExportProgress(TenantInfo tenant, string processId, int progressInPercents)
        {
            var job = GetAllProcesses(tenant).FirstOrDefault(p => p.NaturalId == processId);
            if (job != null)
            {
                job.ProgressInPercents = progressInPercents;
            }
        }

        public void DeleteDataExport(TenantInfo tenant, string processId)
        {
            var job = GetAllProcesses(tenant).FirstOrDefault(p => p.NaturalId == processId);
            job?.Cancel();
        }

        public void ChangeStatusType(TenantInfo tenant, string processId, DataExportStatus status)
        {
            var job = GetAllProcesses(tenant).FirstOrDefault(p => p.NaturalId == processId);
            if (job != null)
            {
                job.Status = status;
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Factory.StartNew(async () =>
            {
                logger.LogInformation("Started in-memory job queue");
                foreach (var job in tempQueue.GetConsumingEnumerable())
                {
                    job.Status = DataExportStatus.Running;
                    logger.LogInformation("Got new job: " + job.NaturalId);
                    currentProcessingJob = job;

                    try
                    {
                        using (var scope = serviceProvider.CreateScope())
                        {
                            var jobToDo = scope.ServiceProvider.GetService<IExportJob>();
                            await jobToDo.ExecuteAsync(job, cancellationToken);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Got error processing job: " + job.NaturalId);
                    }
                    finally
                    {
                        logger.LogInformation("Completed job: " + job.NaturalId);
                        currentProcessingJob = null;
                    }
                }

                logger.LogInformation("Stopped in-memory job queue");
            }, cancellationToken);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Stopping in-memory job queue");
            this.tempQueue.CompleteAdding();
            return Task.CompletedTask;
        }
    }

    public static class DataExportProcessDetailsExtensions
    {
        public static bool IsQueuedOrRunning(this IDataExportProcessDetails process)
        {
            if (process == null) return false;
            return process.Status == DataExportStatus.Queued || process.Status == DataExportStatus.Running ||
                   process.Status == DataExportStatus.Compressing;
        }
    }
}
