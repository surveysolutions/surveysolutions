using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WB.Services.Export.Host.Scheduler.PostgresWorkQueue.Model;
using WB.Services.Export.Services.Processing;
using WB.Services.Export.Tenant;

namespace WB.Services.Export.Host.Scheduler.PostgresWorkQueue.Services.Implementation
{
    class PostgresDataExportProcessesService : IDataExportProcessesService
    {
        private readonly IJobService jobService;
        private readonly IJobProgressReporter jobProgressReporter;
        private readonly ILogger<PostgresDataExportProcessesService> logger;

        public PostgresDataExportProcessesService(
            IJobService jobService,
            IJobProgressReporter jobProgressReporter,
            ILogger<PostgresDataExportProcessesService> logger)
        {
            this.jobService = jobService;
            this.jobProgressReporter = jobProgressReporter;
            this.logger = logger;
        }

        public async Task<long> AddDataExport(DataExportProcessArgs args)
        {
            var job = await this.jobService.AddNewJobAsync(args.Tenant, new JobItem
            {
                Tenant = args.Tenant.Id.ToString(),
                Args = JsonConvert.SerializeObject(args),
                Tag = args.NaturalId,
                Type = JobType.ExportJob,
                ExportState = DataExportStatus.Queued
            });

            return job.Id;
        }

        private DataExportProcessArgs AsDataExportProcessArgs(JobItem job)
        {
            var args = JsonConvert.DeserializeObject<DataExportProcessArgs>(job.Args);

            args.Status = new DataExportProcessStatus
            {
                ProgressInPercents = job.Progress,
                BeginDate = job.StartAt,
                IsRunning = job.Status == JobStatus.Running || job.Status == JobStatus.Created,
                Status = job.ExportState
            };

            return args;
        }

        public async Task<DataExportProcessArgs[]> GetAllProcesses(TenantInfo tenant)
        {
            var jobs = (await this.jobService.GetAllJobs(tenant, JobStatus.Created, JobStatus.Running))
                .Select(AsDataExportProcessArgs).ToArray();

            return jobs;
        }

        public void FinishExportSuccessfully(long processId)
        {
        }

        public void FinishExportWithError(TenantInfo tenant, string tag, Exception e)
        {
            throw new NotImplementedException();
        }

        public Task UpdateDataExportProgressAsync(TenantInfo tenant, string tag, int progressInPercents)
        {
            jobProgressReporter.Add<IJobService>((js, token) => 
                js.UpdateJobAsync(tenant, tag, job =>
            {
                job.Progress = progressInPercents;
            }));

            return Task.CompletedTask;
        }

        public void DeleteDataExport(TenantInfo tenant, string tag)
        {
            jobService.UpdateJobAsync(tenant, tag, j => j.Status = JobStatus.Fail).Wait();
        }

        public Task ChangeStatusTypeAsync(TenantInfo tenant, string tag, DataExportStatus status)
        {
            jobProgressReporter.Add<IJobService>((js, token) =>
            {
                return js.UpdateJobAsync(tenant, tag, job => job.ExportState = status);
            });

            return Task.CompletedTask;
        }
    }
}
