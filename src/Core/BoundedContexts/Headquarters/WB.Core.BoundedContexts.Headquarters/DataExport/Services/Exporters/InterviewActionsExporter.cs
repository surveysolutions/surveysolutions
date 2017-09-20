using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.Export;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services.Exporters
{
    internal class InterviewActionsExporter
    {
        private readonly InterviewDataExportSettings interviewDataExportSettings;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly string interviewActionsFileName = "interview_actions";
        private readonly string[] actionFileColumns = { "InterviewId", "Action", "Originator", "Role", "ResponsibleName", "ResponsibleRole", "Date", "Time" };
        private readonly string dataFileExtension = "tab";
        private readonly ICsvWriter csvWriter;
        private readonly ITransactionManagerProvider transactionManager;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewStatuses;
        private readonly ILogger logger;

        protected InterviewActionsExporter()
        {
        }

        public InterviewActionsExporter(InterviewDataExportSettings interviewDataExportSettings,
            IFileSystemAccessor fileSystemAccessor, 
            ICsvWriter csvWriter, 
            ITransactionManagerProvider transactionManager, 
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewStatuses,
            ILogger logger)
        {
            this.interviewDataExportSettings = interviewDataExportSettings;
            this.fileSystemAccessor = fileSystemAccessor;
            this.csvWriter = csvWriter;
            this.transactionManager = transactionManager;
            this.interviewStatuses = interviewStatuses;
            this.logger = logger;
        }

        public void Export(QuestionnaireIdentity questionnaireIdentity, List<Guid> interviewIdsToExport, string basePath, IProgress<int> progress)
        {
            this.ExportActionsInTabularFormatAsync(interviewIdsToExport, basePath, progress);
        }

        private void ExportActionsInTabularFormatAsync(List<Guid> interviewIdsToExport,
            string basePath,
            IProgress<int> progress)
        {
            var actionFilePath = this.fileSystemAccessor.CombinePath(basePath, Path.ChangeExtension(this.interviewActionsFileName, this.dataFileExtension));

            this.csvWriter.WriteData(actionFilePath, new[] { this.actionFileColumns }, ExportFileSettings.DataFileSeparator.ToString());

            long totalProcessedCount = 0;
            var stopwatch = Stopwatch.StartNew();
            foreach (var interviewsBatch in interviewIdsToExport.Batch(this.interviewDataExportSettings.MaxRecordsCountPerOneExportQuery))
            {
                var interviewIdsStrings = interviewsBatch.Select(x => x).ToArray();
                Expression<Func<InterviewSummary, bool>> whereClauseForAction = 
                    x => interviewIdsStrings.Contains(x.InterviewId);
                string[][] actionsChunk = this.transactionManager.GetTransactionManager().ExecuteInQueryTransaction(() => this.QueryActionsChunkFromReadSide(whereClauseForAction));

                this.csvWriter.WriteData(actionFilePath, actionsChunk, ExportFileSettings.DataFileSeparator.ToString());

                totalProcessedCount += interviewIdsStrings.Length;
                progress.Report(totalProcessedCount.PercentOf(interviewIdsToExport.Count));

                this.logger.Debug($"Exported batch of interview actions. Processed: {totalProcessedCount:N0} out of {interviewIdsToExport.Count:N0}");
            }

            stopwatch.Stop();
            this.logger.Info($"Exported interview actions. Processed: {interviewIdsToExport.Count:N0}. Took {stopwatch.Elapsed:g} to complete");
            progress.Report(100);
        }

        private string[][] QueryActionsChunkFromReadSide(Expression<Func<InterviewSummary, bool>> queryActions)
        {
            var interviews =
              this.interviewStatuses.Query(_ => _
                    .Where(queryActions)
                    .SelectMany(interviewWithStatusHistory => interviewWithStatusHistory.InterviewCommentedStatuses,
                               (interview, status) => new { interview.InterviewId, StatusHistory = status })
                    .Select(i => new 
                    {
                        i.InterviewId,
                        i.StatusHistory.Status,
                        i.StatusHistory.StatusChangeOriginatorName,
                        i.StatusHistory.StatusChangeOriginatorRole,
                        i.StatusHistory.Timestamp,
                        i.StatusHistory.SupervisorName,
                        i.StatusHistory.InterviewerName
                    })
                    .OrderBy(i => i.Timestamp).ToList());

            var result = new List<string[]>();

            foreach (var interview in interviews)
            {
                var resultRow = new List<string>
                {
                    interview.InterviewId.FormatGuid(),
                    interview.Status.ToString(),
                    interview.StatusChangeOriginatorName,
                    this.GetUserRole(interview.StatusChangeOriginatorRole),
                    this.GetResponsibleName(interview.Status, interview.InterviewerName, interview.SupervisorName, interview.StatusChangeOriginatorName),
                    this.GetResponsibleRole(interview.Status, interview.StatusChangeOriginatorRole, interview.InterviewerName),
                    interview.Timestamp.ToString("d", CultureInfo.InvariantCulture),
                    interview.Timestamp.ToString("T", CultureInfo.InvariantCulture)
                };
                result.Add(resultRow.ToArray());
            }
            return result.ToArray();
        }

        private string GetResponsibleName(InterviewExportedAction status, string interviewerName, string supervisorName, string statusChangeOriginatorName)
        {
            switch (status)
            {
                case InterviewExportedAction.Created:
                    return statusChangeOriginatorName;
                case InterviewExportedAction.SupervisorAssigned:
                case InterviewExportedAction.Completed:
                case InterviewExportedAction.RejectedByHeadquarter:
                case InterviewExportedAction.UnapprovedByHeadquarter:
                    return supervisorName;
                case InterviewExportedAction.ApprovedBySupervisor:
                    return Strings.AnyHeadquarters;
                case InterviewExportedAction.ApprovedByHeadquarter:
                    return String.Empty;
            }

            return interviewerName;
        }

        private string GetResponsibleRole(InterviewExportedAction status, UserRoles statusChangeOriginatorRole, string interviewerName)
        {
            switch (status)
            {
                case InterviewExportedAction.Created:
                    return GetUserRole(statusChangeOriginatorRole);
                case InterviewExportedAction.SupervisorAssigned:
                case InterviewExportedAction.Completed:
                case InterviewExportedAction.RejectedByHeadquarter:
                case InterviewExportedAction.UnapprovedByHeadquarter:
                    return FileBasedDataExportRepositoryWriterMessages.Supervisor;
                case InterviewExportedAction.ApprovedBySupervisor:
                    return FileBasedDataExportRepositoryWriterMessages.Headquarter;
                case InterviewExportedAction.ApprovedByHeadquarter:
                    return String.Empty;
                case InterviewExportedAction.InterviewerAssigned:
                    if (string.IsNullOrWhiteSpace(interviewerName))
                        return string.Empty;
                    break;
            }

            return FileBasedDataExportRepositoryWriterMessages.Interviewer;
        }

        private string GetUserRole(UserRoles userRole)
        {
            switch (userRole)
            {
                case UserRoles.Interviewer:
                    return FileBasedDataExportRepositoryWriterMessages.Interviewer;
                case UserRoles.Supervisor:
                    return FileBasedDataExportRepositoryWriterMessages.Supervisor;
                case UserRoles.Headquarter:
                    return FileBasedDataExportRepositoryWriterMessages.Headquarter;
                case UserRoles.Administrator:
                    return FileBasedDataExportRepositoryWriterMessages.Administrator;
                case UserRoles.ApiUser:
                    return FileBasedDataExportRepositoryWriterMessages.ApiUser;

            }
            return FileBasedDataExportRepositoryWriterMessages.UnknownRole;
        }
    }
}