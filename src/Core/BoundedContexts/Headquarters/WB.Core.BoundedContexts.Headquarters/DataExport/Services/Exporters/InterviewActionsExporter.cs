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
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services.Exporters
{
    internal class InterviewActionsExporter
    {
        private readonly InterviewDataExportSettings interviewDataExportSettings;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly string interviewActionsFileName = "interview_actions";
        private readonly string[] actionFileColumns = { "InterviewId", "Action", "Originator", "Role", "Date", "Time" };
        private readonly string dataFileExtension = "tab";
        private readonly ICsvWriter csvWriter;
        private readonly ITransactionManagerProvider transactionManager;
        private readonly IQueryableReadSideRepositoryReader<InterviewStatuses> interviewStatuses;
        private readonly ILogger logger;

        protected InterviewActionsExporter()
        {
        }

        public InterviewActionsExporter(InterviewDataExportSettings interviewDataExportSettings,
            IFileSystemAccessor fileSystemAccessor, 
            ICsvWriter csvWriter, 
            ITransactionManagerProvider transactionManager, 
            IQueryableReadSideRepositoryReader<InterviewStatuses> interviewStatuses,
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

            this.csvWriter.WriteData(actionFilePath, new[] { this.actionFileColumns }, ExportFileSettings.SeparatorOfExportedDataFile.ToString());

            int totalProcessedCount = 0;
            var stopwatch = Stopwatch.StartNew();
            foreach (var interviewsBatch in interviewIdsToExport.Batch(this.interviewDataExportSettings.MaxRecordsCountPerOneExportQuery))
            {
                var interviewIdsStrings = interviewsBatch.Select(x => x.FormatGuid()).ToArray();
                Expression<Func<InterviewStatuses, bool>> whereClauseForAction = 
                    x => interviewIdsStrings.Contains(x.InterviewId);
                var actionsChunk = this.transactionManager.GetTransactionManager().ExecuteInQueryTransaction(() => this.QueryActionsChunkFromReadSide(whereClauseForAction));

                this.csvWriter.WriteData(actionFilePath, actionsChunk, ExportFileSettings.SeparatorOfExportedDataFile.ToString());

                totalProcessedCount += interviewIdsStrings.Length;
                progress.Report(totalProcessedCount.PercentOf(interviewIdsToExport.Count));

                this.logger.Debug($"Exported batch of interview actions. Processed: {totalProcessedCount:N0} out of {interviewIdsToExport.Count:N0}");
            }

            stopwatch.Stop();
            this.logger.Info($"Exported interview actions. Processed: {interviewIdsToExport.Count:N0}. Took {stopwatch.Elapsed:g} to complete");
            progress.Report(100);
        }

        private string[][] QueryActionsChunkFromReadSide(Expression<Func<InterviewStatuses, bool>> queryActions)
        {
            var interviews =
              this.interviewStatuses.Query(
                  _ =>
                      _.Where(queryActions)
                          .SelectMany(
                              interviewWithStatusHistory => interviewWithStatusHistory.InterviewCommentedStatuses,
                              (interview, status) => new { interview.InterviewId, StatusHistory = status })
                          .Select(
                              i =>
                                  new
                                  {
                                      i.InterviewId,
                                      i.StatusHistory.Status,
                                      i.StatusHistory.StatusChangeOriginatorName,
                                      i.StatusHistory.StatusChangeOriginatorRole,
                                      i.StatusHistory.Timestamp

                                  }).OrderBy(i => i.Timestamp).ToList());

            var result = new List<string[]>();

            foreach (var interview in interviews)
            {
                var resultRow = new List<string>
                {
                    interview.InterviewId,
                    interview.Status.ToString(),
                    interview.StatusChangeOriginatorName,
                    this.GetUserRole(interview.StatusChangeOriginatorRole),
                    interview.Timestamp.ToString("d", CultureInfo.InvariantCulture),
                    interview.Timestamp.ToString("T", CultureInfo.InvariantCulture)
                };
                result.Add(resultRow.ToArray());
            }
            return result.ToArray();
        }

        private string GetUserRole(UserRoles userRole)
        {
            switch (userRole)
            {
                case UserRoles.Operator:
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