using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.GenericSubdomains.Portable;
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

        protected InterviewActionsExporter()
        {
        }

        public InterviewActionsExporter(InterviewDataExportSettings interviewDataExportSettings,
            IFileSystemAccessor fileSystemAccessor, 
            ICsvWriter csvWriter, 
            ITransactionManagerProvider transactionManager, IQueryableReadSideRepositoryReader<InterviewStatuses> interviewStatuses)
        {
            this.interviewDataExportSettings = interviewDataExportSettings;
            this.fileSystemAccessor = fileSystemAccessor;
            this.csvWriter = csvWriter;
            this.transactionManager = transactionManager;
            this.interviewStatuses = interviewStatuses;
        }

        public void ExportAll(QuestionnaireIdentity questionnaireIdentity, string basePath, IProgress<int> progress)
        {
            Expression<Func<InterviewStatuses, bool>> whereClauseForAction =
            interviewWithStatusHistory =>
                interviewWithStatusHistory.QuestionnaireId == questionnaireIdentity.QuestionnaireId &&
                interviewWithStatusHistory.QuestionnaireVersion == questionnaireIdentity.Version;

            this.ExportActionsInTabularFormatAsync(whereClauseForAction, basePath, progress);
        }

        public void ExportApproved(QuestionnaireIdentity questionnaireIdentity, string basePath, IProgress<int> progress)
        {
            Expression<Func<InterviewStatuses, bool>> whereClauseForAction =
                interviewWithStatusHistory =>
                    interviewWithStatusHistory.QuestionnaireId == questionnaireIdentity.QuestionnaireId &&
                    interviewWithStatusHistory.QuestionnaireVersion == questionnaireIdentity.Version &&
                    interviewWithStatusHistory.InterviewCommentedStatuses.Select(s => s.Status)
                        .Any(s => s == InterviewExportedAction.ApprovedByHeadquarter);

            this.ExportActionsInTabularFormatAsync(whereClauseForAction, basePath, progress);

        }

        private void ExportActionsInTabularFormatAsync(Expression<Func<InterviewStatuses, bool>> whereClauseForAction,
            string basePath,
            IProgress<int> progress)
        {
            var actionFilePath = this.fileSystemAccessor.CombinePath(basePath, Path.ChangeExtension(this.interviewActionsFileName, this.dataFileExtension));

            this.csvWriter.WriteData(actionFilePath, new[] { this.actionFileColumns }, ExportFileSettings.SeparatorOfExportedDataFile.ToString());

            var totalActionsToExportCount = this.CountActionsToExport(whereClauseForAction);

            int skip = 0;

            while (skip < totalActionsToExportCount)
            {
                var skipAtCurrentIteration = skip;

                var chunk = this.transactionManager.GetTransactionManager().ExecuteInQueryTransaction(() => this.QueryActionsChunkFromReadSide(whereClauseForAction, skipAtCurrentIteration));

                this.csvWriter.WriteData(actionFilePath, chunk, ExportFileSettings.SeparatorOfExportedDataFile.ToString());
                skip = skip + this.interviewDataExportSettings.MaxRecordsCountPerOneExportQuery;

                progress.Report((skipAtCurrentIteration+chunk.Length).PercentOf(totalActionsToExportCount));
            }

            progress.Report(100);
        }

        private int CountActionsToExport(Expression<Func<InterviewStatuses, bool>> whereClauseForAction)
        {
            return this.transactionManager.GetTransactionManager().ExecuteInQueryTransaction(() => 
                this.interviewStatuses.Query(_ => _.Where(whereClauseForAction)
                    .SelectMany(x => x.InterviewCommentedStatuses)
                    .Count()));
        }

        private string[][] QueryActionsChunkFromReadSide(Expression<Func<InterviewStatuses, bool>> queryActions, int skip)
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

                                  }).OrderBy(i => i.Timestamp).Skip(skip).Take(this.interviewDataExportSettings.MaxRecordsCountPerOneExportQuery).ToList());

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
            }
            return FileBasedDataExportRepositoryWriterMessages.UnknownRole;
        }
    }
}