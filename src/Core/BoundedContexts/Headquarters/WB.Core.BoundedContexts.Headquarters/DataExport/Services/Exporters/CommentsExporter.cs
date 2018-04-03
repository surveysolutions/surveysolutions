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
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services.Exporters
{
    internal class CommentsExporter
    {
        private readonly InterviewDataExportSettings interviewDataExportSettings;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ICsvWriter csvWriter;
        private readonly string dataFileExtension = "tab";
        private readonly string commentsFileName = "interview__comments";
        private readonly IQueryableReadSideRepositoryReader<InterviewCommentaries> interviewCommentariesStorage;
        private readonly ITransactionManagerProvider transactionManager;
        private readonly ILogger logger;

        protected CommentsExporter()
        {
        }

        public CommentsExporter(
            InterviewDataExportSettings interviewDataExportSettings,
            IFileSystemAccessor fileSystemAccessor,
            ICsvWriter csvWriter,
            IQueryableReadSideRepositoryReader<InterviewCommentaries> interviewCommentariesStorage,
            ITransactionManagerProvider transactionManager,
            ILogger logger)
        {
            this.interviewDataExportSettings = interviewDataExportSettings;
            this.fileSystemAccessor = fileSystemAccessor;
            this.csvWriter = csvWriter;
            this.interviewCommentariesStorage = interviewCommentariesStorage;
            this.transactionManager = transactionManager;
            this.logger = logger;
        }

        public void Export(QuestionnaireExportStructure questionnaireExportStructure, List<Guid> interviewIdsToExport, string basePath, IProgress<int> progress)
        {
            this.DoExport(questionnaireExportStructure, interviewIdsToExport, basePath, progress);
        }

        private void DoExport(QuestionnaireExportStructure questionnaireExportStructure,
            List<Guid> interviewIdsToExport,
            string basePath,
            IProgress<int> progress)
        {
            var batchSize = this.interviewDataExportSettings.MaxRecordsCountPerOneExportQuery;

            
            string commentsFilePath = this.fileSystemAccessor.CombinePath(basePath, Path.ChangeExtension(this.commentsFileName, this.dataFileExtension));
            int maxRosterDepthInQuestionnaire = questionnaireExportStructure.HeaderToLevelMap.Values.Max(x => x.LevelScopeVector.Count);
            bool hasAtLeastOneRoster = questionnaireExportStructure.HeaderToLevelMap.Values.Any(x => x.LevelScopeVector.Count > 0);
            this.WriteFileHeader(hasAtLeastOneRoster, maxRosterDepthInQuestionnaire, commentsFilePath);

            long totalProcessed = 0;
            var stopwatch = Stopwatch.StartNew();
            var etaHelper = new EtaHelper(interviewIdsToExport.Count, batchSize, trackingStopwatch: stopwatch);
            
            foreach (var interviewsChunk in interviewIdsToExport.Batch(batchSize))
            {
                var interviewIdsStrings = interviewsChunk.Select(x => x.FormatGuid()).ToArray();

                Expression<Func<InterviewCommentaries, bool>> whereClauseForComments = 
                    x => interviewIdsStrings.Contains(x.InterviewId);
                
                var exportComments = this.transactionManager
                                           .GetTransactionManager()
                                           .ExecuteInQueryTransaction(
                                                () => this.QueryCommentsChunkFromReadSide(whereClauseForComments, maxRosterDepthInQuestionnaire, hasAtLeastOneRoster));

                this.csvWriter.WriteData(commentsFilePath, exportComments, ExportFileSettings.DataFileSeparator.ToString());
                totalProcessed += interviewIdsStrings.Length;

                etaHelper.AddProgress(interviewIdsStrings.Length);

                progress.Report(totalProcessed.PercentOf(interviewIdsToExport.Count));
                this.logger.Debug($"Exported batch of interview comments. {etaHelper} " +
                                  $"Total interview comments processed {totalProcessed:N0} out of {interviewIdsToExport.Count:N0}");
            }
            stopwatch.Stop();
            this.logger.Info($"Exported all interview comments. Took {stopwatch.Elapsed:g} to export {interviewIdsToExport.Count:N0} interviews");
            progress.Report(100);
        }

        private void WriteFileHeader(bool hasAtLeastOneRoster, int maxRosterDepthInQuestionnaire, string commentsFilePath)
        {
            var commentsHeader = new List<string> {"Order", "Originator", "Role", "Date", "Time", "Variable"};

            if (hasAtLeastOneRoster)
                commentsHeader.Add("Roster");

            commentsHeader.Add("interview__id");

            for (int i = 1; i <= maxRosterDepthInQuestionnaire; i++)
            {
                commentsHeader.Add($"Id{i}");
            }
            commentsHeader.Add("Comment");

            this.csvWriter.WriteData(commentsFilePath, new[] {commentsHeader.ToArray()}, ExportFileSettings.DataFileSeparator.ToString());
        }

        private List<string[]> QueryCommentsChunkFromReadSide(Expression<Func<InterviewCommentaries, bool>> queryComments, 
            int maxRosterDepthInQuestionnaire, bool hasAtLeastOneRoster)
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
                                            }).OrderBy(i => i.Timestamp).ToList());

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
            return result;
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