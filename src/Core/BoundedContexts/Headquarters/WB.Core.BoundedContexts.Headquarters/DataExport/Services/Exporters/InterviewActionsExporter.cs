using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Dapper;
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
using WB.Infrastructure.Native.Storage.Postgre.Implementation;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services.Exporters
{
    internal class InterviewActionsExporter
    {
        private readonly InterviewDataExportSettings interviewDataExportSettings;
        private readonly IFileSystemAccessor fileSystemAccessor;
        public readonly string InterviewActionsFileName = "interview__actions";
        public readonly DoExportFileHeader[] ActionFileColumns =
        {
            new DoExportFileHeader("interview__id", "Unique 32-character long identifier of the interview"), 
            new DoExportFileHeader("interview__key", "Identifier of the interview"), 
            new DoExportFileHeader("Action", "Type of action taken"), 
            new DoExportFileHeader("Originator", "Login name of the person performing the action"), 
            new DoExportFileHeader("Role", "System role of the person performing the action"), 
            new DoExportFileHeader("ResponsibleName", "Login name of the person now responsible for the interview"), 
            new DoExportFileHeader("ResponsibleRole", "System role of the person now responsible for the interview"), 
            new DoExportFileHeader("Date", "Date when the action was taken"), 
            new DoExportFileHeader("Time", "Time when the action was taken")
        };
        private readonly string dataFileExtension = "tab";
        private readonly ICsvWriter csvWriter;
        private readonly ITransactionManagerProvider transactionManager;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewStatuses;
        private readonly ISessionProvider sessionProvider;
        private readonly ILogger logger;

        protected InterviewActionsExporter()
        {

        }

        public InterviewActionsExporter(InterviewDataExportSettings interviewDataExportSettings,
            IFileSystemAccessor fileSystemAccessor, 
            ICsvWriter csvWriter, 
            ITransactionManagerProvider transactionManager, 
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewStatuses,
            ILogger logger, ISessionProvider sessionProvider)
        {
            this.interviewDataExportSettings = interviewDataExportSettings;
            this.fileSystemAccessor = fileSystemAccessor;
            this.csvWriter = csvWriter;
            this.transactionManager = transactionManager;
            this.interviewStatuses = interviewStatuses;
            this.logger = logger;
            this.sessionProvider = sessionProvider;
        }

        public void Export(QuestionnaireIdentity questionnaireIdentity, List<Guid> interviewIdsToExport, string basePath, IProgress<int> progress)
        {
            var actionFilePath = this.fileSystemAccessor.CombinePath(basePath, Path.ChangeExtension(this.InterviewActionsFileName, this.dataFileExtension));
            var batchSize = this.interviewDataExportSettings.MaxRecordsCountPerOneExportQuery;

            var fileColumns = this.ActionFileColumns.Select(a => a.Title).ToArray();
            this.csvWriter.WriteData(actionFilePath, new[] { fileColumns }, ExportFileSettings.DataFileSeparator.ToString());

            long totalProcessedCount = 0;
            var stopwatch = Stopwatch.StartNew();
            var etaHelper = new EtaHelper(interviewIdsToExport.Count, batchSize, trackingStopwatch: stopwatch);

            foreach (var interviewsBatch in interviewIdsToExport.Batch(batchSize))
            {
                var interviewIdsStrings = interviewsBatch.ToArray();
                Expression<Func<InterviewSummary, bool>> whereClauseForAction = x => interviewIdsStrings.Contains(x.InterviewId);

                var actionsChunk = this.transactionManager.GetTransactionManager().ExecuteInQueryTransaction(
                    () => this.QueryActionsChunkFromReadSide(whereClauseForAction));

                this.csvWriter.WriteData(actionFilePath, actionsChunk, ExportFileSettings.DataFileSeparator.ToString());

                totalProcessedCount += interviewIdsStrings.Length;
                progress.Report(totalProcessedCount.PercentOf(interviewIdsToExport.Count));
                etaHelper.AddProgress(interviewIdsStrings.Length);

                this.logger.Debug($"Exported batch of interview actions. Processed: {totalProcessedCount:N0} out of {interviewIdsToExport.Count:N0}. {etaHelper}");
            }

            stopwatch.Stop();
            this.logger.Info($"Exported interview actions. Processed: {interviewIdsToExport.Count:N0}. Took {stopwatch.Elapsed:g} to complete");
            progress.Report(100);
        }

        public void ExportActionsDoFile(string basePath)
        {
            var doContent = new DoFile();

            doContent.BuildInsheet(Path.ChangeExtension(this.InterviewActionsFileName, this.dataFileExtension));
            doContent.AppendLine();

            foreach (var actionFileColumn in ActionFileColumns)
            {
                doContent.AppendLabelToVariableMatching(actionFileColumn.Title, actionFileColumn.Description);
            }

            var fileName = $"{InterviewActionsFileName}.{DoFile.ContentFileNameExtension}";
            var contentFilePath = this.fileSystemAccessor.CombinePath(basePath, fileName);

            this.fileSystemAccessor.WriteAllText(contentFilePath, doContent.ToString());
        }

        private List<string[]> QueryActionsChunkFromReadSide(Expression<Func<InterviewSummary, bool>> queryActions)
        {
            sessionProvider.GetSession()?.Connection?.Execute("set enable_seqscan=false");

            var interviews =
              this.interviewStatuses.Query(_ => _
                    .Where(queryActions)
                    .SelectMany(interviewWithStatusHistory => interviewWithStatusHistory.InterviewCommentedStatuses,
                               (interview, status) => new { interview.InterviewId, interview.Key, StatusHistory = status })
                    .Select(i => new 
                    {
                        i.InterviewId,
                        i.Key,
                        i.StatusHistory.Status,
                        i.StatusHistory.StatusChangeOriginatorName,
                        i.StatusHistory.StatusChangeOriginatorRole,
                        i.StatusHistory.Timestamp,
                        i.StatusHistory.SupervisorName,
                        i.StatusHistory.InterviewerName,
                        i.StatusHistory.Position
                    })
                    .OrderBy(x => x.InterviewId)
                        .ThenBy(x => x.Position).ToList());

            var result = new List<string[]>();

            foreach (var interview in interviews)
            {
                var resultRow = new List<string>
                {
                    interview.InterviewId.FormatGuid(),
                    interview.Key,
                    interview.Status.ToString(),
                    interview.StatusChangeOriginatorName,
                    this.GetUserRole(interview.StatusChangeOriginatorRole),
                    this.GetResponsibleName(interview.Status, interview.InterviewerName, interview.SupervisorName, interview.StatusChangeOriginatorName),
                    this.GetResponsibleRole(interview.Status, interview.StatusChangeOriginatorRole, interview.InterviewerName),
                    interview.Timestamp.ToString(ExportFormatSettings.ExportDateFormat, CultureInfo.InvariantCulture),
                    interview.Timestamp.ToString("T", CultureInfo.InvariantCulture)
                };
                result.Add(resultRow.ToArray());
            }
            return result;
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
                case InterviewExportedAction.OpenedBySupervisor:
                case InterviewExportedAction.ClosedBySupervisor:
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
