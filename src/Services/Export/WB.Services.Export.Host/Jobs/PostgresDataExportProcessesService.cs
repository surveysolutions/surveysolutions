﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Models;
using WB.Services.Export.Services.Processing;
using WB.Services.Infrastructure.Logging;
using WB.Services.Scheduler;
using WB.Services.Scheduler.Model;
using WB.Services.Scheduler.Services;

namespace WB.Services.Export.Host.Jobs
{
    class PostgresDataExportProcessesService : IDataExportProcessesService
    {
        private readonly IJobService jobService;
        private readonly IJobProgressReporter jobProgressReporter;
        private readonly ILogger<PostgresDataExportProcessesService> logger;
        private readonly IOptions<JobSettings> jobSettings;
        private readonly ITenantContext tenantContext;
        private readonly TenantDbContext tenantDbContext;

        public PostgresDataExportProcessesService(
            IJobService jobService,
            IJobProgressReporter jobProgressReporter,
            ILogger<PostgresDataExportProcessesService> logger,
            IOptions<JobSettings> jobSettings,
            ITenantContext tenantContext,
            TenantDbContext tenantDbContext)
        {
            this.jobService = jobService;
            this.jobProgressReporter = jobProgressReporter;
            this.logger = logger;
            this.jobSettings = jobSettings;
            this.tenantContext = tenantContext;
            this.tenantDbContext = tenantDbContext;
        }

        public async Task<long> AddDataExport(DataExportProcessArgs args)
        {
            using (LoggingHelpers.LogContext(("tenantName", args.ExportSettings.Tenant.Name)))
            {
                var job = await this.jobService.AddNewJobAsync(new JobItem
                {
                    MaxRetryAttempts = this.jobSettings.Value.MaxRetryAttempts,
                    Tenant = args.ExportSettings.Tenant.ToString(),
                    TenantName = args.ExportSettings.Tenant.Name,
                    Args = JsonConvert.SerializeObject(args),
                    Tag = args.NaturalId,
                    Type = ExportJobRunner.Name,
                    Data =
                    {
                        [StatusField] = DataExportStatus.Queued.ToString()
                    }
                });

                return job.Id;
            }
        }
        
        public async Task<List<DataExportProcessArgs>> GetAllProcesses(bool runningOnly = true)
        {
            var jobItems = runningOnly
                ? await this.jobService.GetRunningOrQueuedJobs(tenantContext.Tenant)
                : await this.jobService.GetAllJobsAsync(tenantContext.Tenant);

            return await AsDataExportProcesses(jobItems);
        }

        private async Task EnsureMigrated(CancellationToken cancellationToken)
        {
            if (this.tenantDbContext.Database.IsNpgsql())
            {
                await this.tenantDbContext.CheckSchemaVersionAndMigrate(cancellationToken);
                await this.tenantDbContext.SetContextSchema(cancellationToken);
            }
        }

        private HashSet<string> deletedQuestionnaires;
        private HashSet<string> DeletedQuestionnaires =>
             deletedQuestionnaires ??= this.tenantDbContext.GeneratedQuestionnaires
                    .Where(q => q.DeletedAt != null)
                    .Select(q => q.Id)
                    .ToHashSet();

        private async Task<List<DataExportProcessArgs>> AsDataExportProcesses(IEnumerable<JobItem> jobItems, 
            CancellationToken cancellationToken = default)
        {
            await EnsureMigrated(cancellationToken);
            return jobItems.Select(j => AsDataExportProcessArgs(j))
                .Where(d => !DeletedQuestionnaires.Contains(d.ExportSettings.QuestionnaireId.Id))
                .ToList();
        }

        public async Task<DataExportProcessArgs> GetProcessAsync(long processId)
        {
            var job = await this.jobService.GetJobAsync(processId);
            if (job == null || job.Tenant != this.tenantContext.Tenant.Id.Id) return null;

            return AsDataExportProcessArgs(job);
        }

        DataExportProcessArgs AsDataExportProcessArgs(JobItem job)
        {
            var args = JsonConvert.DeserializeObject<DataExportProcessArgs>(job.Args);

            var eta = job.GetData<string>(EtaField);
            var status = Enum.Parse<DataExportStatus>(job.GetData<string>(StatusField));
            var hasError = /*job.Status == JobStatus.Canceled ||*/ job.Status == JobStatus.Fail;

            args.Status = new DataExportProcessStatus
            {
                TimeEstimation = eta == null ? (TimeSpan?)null : TimeSpan.Parse(eta),
                CreatedDate = job.CreatedAt,
                BeginDate = job.StartAt ?? job.CreatedAt,
                EndDate = job.EndAt,
                IsRunning = job.Status == JobStatus.Running || job.Status == JobStatus.Created,
                Status = status,
                JobStatus = (DataExportJobStatus)job.Status,
                ProgressInPercents = Int32.Parse(job.GetData<string>(ProgressField) ?? "0"),
                Error = hasError
                    ? new DateExportProcessError
                    {
                        Type = Enum.Parse<DataExportError>(job.GetData<string>(ErrorTypeField) ??
                                                           DataExportError.Unexpected.ToString()),
                        Message = job.GetData<string>(ErrorField)
                    }
                    : null
            };

            args.ProcessId = job.Id;
            return args;
        }

        public async Task<List<DataExportProcessArgs>> GetProcessesAsync(long[] processIds)
        {
            var jobs = await this.jobService.GetJobsAsync(processIds);
            return await AsDataExportProcesses(jobs);
        }

        public void UpdateDataExportProgress(long processId, int progressInPercents, TimeSpan estimatedTime = default)
        {
            logger.LogTrace("Update progress: {progressInPercents}%", progressInPercents);
            jobProgressReporter.UpdateJobData(processId, ProgressField, progressInPercents.ToString());

            if (estimatedTime != default)
            {
                jobProgressReporter.UpdateJobData(processId, EtaField, estimatedTime);
            }
        }

        public void DeleteDataExport(long processId, string reason)
        {
            jobProgressReporter.CancelJob(processId, reason);
        }

        public void ChangeStatusType(long processId, DataExportStatus status)
        {
            jobProgressReporter.UpdateJobData(processId, StatusField, status.ToString());
        }

        public const string StatusField = "exportStatus";
        public const string ProgressField = "progress";
        public const string EtaField = "eta";
        public const string ErrorTypeField = "errorType";
        public const string ErrorField = "error";
    }
}
