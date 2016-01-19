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
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services.Exporters
{
    internal class CommentsExporter
    {
        private readonly InterviewDataExportSettings interviewDataExportSettings;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ICsvWriter csvWriter;
        private readonly string dataFileExtension = "tab";
        private readonly string commentsFileName = "interview_comments";
        private readonly IQueryableReadSideRepositoryReader<InterviewCommentaries> interviewCommentariesStorage;
        private readonly ITransactionManagerProvider transactionManager;

        protected CommentsExporter()
        {
        }

        public CommentsExporter(
            InterviewDataExportSettings interviewDataExportSettings,
            IFileSystemAccessor fileSystemAccessor,
            ICsvWriter csvWriter,
            IQueryableReadSideRepositoryReader<InterviewCommentaries> interviewCommentariesStorage,
            ITransactionManagerProvider transactionManager)
        {
            this.interviewDataExportSettings = interviewDataExportSettings;
            this.fileSystemAccessor = fileSystemAccessor;
            this.csvWriter = csvWriter;
            this.interviewCommentariesStorage = interviewCommentariesStorage;
            this.transactionManager = transactionManager;
        }

        public void Export(QuestionnaireExportStructure questionnaireExportStructure,
            string basePath,
            IProgress<int> progress)
        {
            this.DoExport(questionnaireExportStructure, false, basePath, progress);
        }

        public void ExportApproved(QuestionnaireExportStructure questionnaireExportStructure,
            string basePath,
            IProgress<int> progress)
        {
            this.DoExport(questionnaireExportStructure, true, basePath, progress);
        }

        private void DoExport(QuestionnaireExportStructure questionnaireExportStructure,
            bool exportApprovedOnly,
            string basePath,
            IProgress<int> progress)
        {
            string commentsFilePath =
                this.fileSystemAccessor.CombinePath(basePath, Path.ChangeExtension(this.commentsFileName, this.dataFileExtension));

            int maxRosterDepthInQuestionnaire =
                questionnaireExportStructure.HeaderToLevelMap.Values.Max(x => x.LevelScopeVector.Count);

            bool hasAtLeastOneRoster =
                questionnaireExportStructure.HeaderToLevelMap.Values.Any(x => x.LevelScopeVector.Count > 0);

            var commentsHeader = new List<string> { "Order", "Originator", "Role", "Date", "Time", "Variable" };

            if (hasAtLeastOneRoster)
                commentsHeader.Add("Roster");

            commentsHeader.Add("InterviewId");

            for (int i = 1; i <= maxRosterDepthInQuestionnaire; i++)
            {
                commentsHeader.Add($"Id{i}");
            }
            commentsHeader.Add("Comment");

            this.csvWriter.WriteData(commentsFilePath, new[] { commentsHeader.ToArray() }, ExportFileSettings.SeparatorOfExportedDataFile.ToString());

            Expression<Func<InterviewCommentaries, bool>> whereClauseForComments;
            if (exportApprovedOnly)
            {
                whereClauseForComments =
                    interviewComments =>
                        interviewComments.QuestionnaireId == questionnaireExportStructure.QuestionnaireId.FormatGuid() &&
                        interviewComments.QuestionnaireVersion == questionnaireExportStructure.Version &&
                        interviewComments.IsApprovedByHQ;
            }
            else
            {
                whereClauseForComments =
                    interviewComments =>
                        interviewComments.QuestionnaireId == questionnaireExportStructure.QuestionnaireId.FormatGuid() &&
                        interviewComments.QuestionnaireVersion == questionnaireExportStructure.Version;
            }

            var countOfAllRecords =
                this.transactionManager.GetTransactionManager()
                    .ExecuteInQueryTransaction(() => this.interviewCommentariesStorage.Query(_ => _.Where(whereClauseForComments).SelectMany(x => x.Commentaries).Count()));

            int skip = 0;

            while (skip < countOfAllRecords)
            {
                var skipAtCurrentIteration = skip;

                string[][] exportComments = this.transactionManager.GetTransactionManager()
                                                .ExecuteInQueryTransaction(
                                                     () => this.QueryCommentsChunkFromReadSide(whereClauseForComments, skipAtCurrentIteration, maxRosterDepthInQuestionnaire, hasAtLeastOneRoster));

                this.csvWriter.WriteData(commentsFilePath, exportComments, ExportFileSettings.SeparatorOfExportedDataFile.ToString());
                skip = skip + this.interviewDataExportSettings.MaxRecordsCountPerOneExportQuery;

                progress.Report((skipAtCurrentIteration+ exportComments.Length).PercentOf(countOfAllRecords));
            }

            progress.Report(100);
        }

        private string[][] QueryCommentsChunkFromReadSide(Expression<Func<InterviewCommentaries, bool>> queryComments, int skip, int maxRosterDepthInQuestionnaire, bool hasAtLeastOneRoster)
        {
            var comments = this.interviewCommentariesStorage.Query(
                            _ =>
                                _.Where(queryComments)
                                    .SelectMany(
                                        interviewComments => interviewComments.Commentaries,
                                        (interview, comment) => new { interview.InterviewId, Comments = comment })
                                    .Select(
                                        i =>
                                            new
                                            {
                                                i.InterviewId,
                                                i.Comments.CommentSequence,
                                                i.Comments.OriginatorName,
                                                i.Comments.OriginatorRole,
                                                i.Comments.Timestamp,
                                                i.Comments.Variable,
                                                i.Comments.Roster,
                                                i.Comments.RosterVector,
                                                i.Comments.Comment
                                            }).OrderBy(i => i.Timestamp).Skip(skip).Take(this.interviewDataExportSettings.MaxRecordsCountPerOneExportQuery).ToList());

            var result = new List<string[]>();

            foreach (var interview in comments)
            {
                var resultRow = new List<string>
                {
                    interview.CommentSequence.ToString(),
                    interview.OriginatorName,
                    this.GetUserRole(interview.OriginatorRole),
                    interview.Timestamp.ToString("d", CultureInfo.InvariantCulture),
                    interview.Timestamp.ToString("T", CultureInfo.InvariantCulture),
                    interview.Variable
                };

                if (hasAtLeastOneRoster)
                    resultRow.Add(interview.Roster);

                resultRow.Add(interview.InterviewId);

                for (int i = 0; i < maxRosterDepthInQuestionnaire; i++)
                {
                    resultRow.Add(interview.RosterVector.Length > i ? interview.RosterVector[i].ToString(CultureInfo.InvariantCulture) : "");
                }

                resultRow.Add(interview.Comment);

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