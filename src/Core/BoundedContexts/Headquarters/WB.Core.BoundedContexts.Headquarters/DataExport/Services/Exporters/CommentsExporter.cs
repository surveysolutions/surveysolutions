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

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services.Exporters
{
    internal class CommentsExporter
    {
        private readonly InterviewDataExportSettings interviewDataExportSettings;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ICsvWriter csvWriter;
        private readonly string dataFileExtension = "tab";
        public readonly string CommentsFileName = "interview__comments";
        private readonly IQueryableReadSideRepositoryReader<InterviewCommentaries> interviewCommentariesStorage;
       private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryStorage;
        private readonly ILogger logger;

        public readonly DoExportFileHeader[] CommentsFileColumns =
        {
            new DoExportFileHeader("Order", "Sequential order of the comment"),
            new DoExportFileHeader("Originator", "Login name of the person leaving the comment"),
            new DoExportFileHeader("Role", "System role of the person leaving the comment"),
            new DoExportFileHeader("Date", "Date when the comment was left"),
            new DoExportFileHeader("Time", "Time when the comment was left"),
            new DoExportFileHeader("Variable", "Variable name for the commented question"),
            new DoExportFileHeader("interview__id", "Unique 32-character long identifier of the interview"),
            new DoExportFileHeader("interview__key", "Identifier of the interview"),
            new DoExportFileHeader("Comment", "Text of the comment"),
            new DoExportFileHeader("Roster", "Name of the roster containing the variable"),
            new DoExportFileHeader("Id1", "Roster ID of the 1st level of nesting", true),
            new DoExportFileHeader("Id2", "Roster ID of the 2nd level of nesting", true),
            new DoExportFileHeader("Id3", "Roster ID of the 3rd level of nesting", true),
            new DoExportFileHeader("Id4", "Roster ID of the 4th level of nesting", true),
        };

        protected CommentsExporter()
        {
        }

        public CommentsExporter(
            InterviewDataExportSettings interviewDataExportSettings,
            IFileSystemAccessor fileSystemAccessor,
            ICsvWriter csvWriter,
            IQueryableReadSideRepositoryReader<InterviewCommentaries> interviewCommentariesStorage,
            ILogger logger, 
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryStorage)
        {
            this.interviewDataExportSettings = interviewDataExportSettings;
            this.fileSystemAccessor = fileSystemAccessor;
            this.csvWriter = csvWriter;
            this.interviewCommentariesStorage = interviewCommentariesStorage;
            this.logger = logger;
            this.interviewSummaryStorage = interviewSummaryStorage;
        }

        public void Export(QuestionnaireExportStructure questionnaireExportStructure,
            List<Guid> interviewIdsToExport,
            string basePath,
            IProgress<int> progress)
        {
            var batchSize = this.interviewDataExportSettings.MaxRecordsCountPerOneExportQuery;

            
            string commentsFilePath = this.fileSystemAccessor.CombinePath(basePath, Path.ChangeExtension(this.CommentsFileName, this.dataFileExtension));
            int maxRosterDepthInQuestionnaire = questionnaireExportStructure.HeaderToLevelMap.Values.Max(x => x.LevelScopeVector.Count);
            bool hasAtLeastOneRoster = questionnaireExportStructure.HeaderToLevelMap.Values.Any(x => x.LevelScopeVector.Count > 0);
            this.WriteFileHeader(hasAtLeastOneRoster, maxRosterDepthInQuestionnaire, commentsFilePath);

            long totalProcessed = 0;
            var stopwatch = Stopwatch.StartNew();
            var etaHelper = new EtaHelper(interviewIdsToExport.Count, batchSize, trackingStopwatch: stopwatch);
            
            foreach (var interviewsChunk in interviewIdsToExport.Batch(batchSize))
            {
                var interviewIdsChunk = interviewsChunk.ToHashSet();
                var interviewIdsStrings = interviewIdsChunk.Select(x => x.FormatGuid()).ToArray();
                
                Expression<Func<InterviewCommentaries, bool>> whereClauseForComments = 
                    x => interviewIdsStrings.Contains(x.InterviewId);

                var interviewKeysMap = this.transactionManager
                    .GetTransactionManager()
                    .ExecuteInQueryTransaction(() =>
                        this.interviewSummaryStorage.Query(_ => _
                            .Where(x => interviewIdsChunk.Contains(x.InterviewId))
                            .Select(x => new {x.InterviewId, x.Key})
                            .ToList()))
                    .ToDictionary(x => x.InterviewId.FormatGuid(), x => x.Key);

                var exportComments = this.transactionManager
                                           .GetTransactionManager()
                                           .ExecuteInQueryTransaction(
                                                () => this.QueryCommentsChunkFromReadSide(whereClauseForComments, maxRosterDepthInQuestionnaire, hasAtLeastOneRoster, interviewKeysMap));

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

        public void ExportCommentsDoFile(QuestionnaireExportStructure questionnaireExportStructure, string basePath)
        {
            var doContent = new DoFile();

            doContent.BuildInsheet(Path.ChangeExtension(this.CommentsFileName, this.dataFileExtension));
            doContent.AppendLine();

            int maxRosterDepthInQuestionnaire = questionnaireExportStructure.HeaderToLevelMap.Values.Max(x => x.LevelScopeVector.Count);
            bool hasAtLeastOneRoster = questionnaireExportStructure.HeaderToLevelMap.Values.Any(x => x.LevelScopeVector.Count > 0);
            var headersList = this.GetHeadersList(hasAtLeastOneRoster, maxRosterDepthInQuestionnaire);

            foreach (var header in headersList)
            {
                var exportFileHeader = CommentsFileColumns.SingleOrDefault(c => c.Title.Equals(header, StringComparison.CurrentCultureIgnoreCase));
                if (exportFileHeader != null)
                {
                    if (exportFileHeader.AddCapture)
                        doContent.AppendCaptureLabelToVariableMatching(exportFileHeader.Title, exportFileHeader.Description);
                    else
                        doContent.AppendLabelToVariableMatching(exportFileHeader.Title, exportFileHeader.Description);
                }
                else
                {
                    doContent.AppendLabelToVariableMatching(header, string.Empty);
                }
            }

            var fileName = $"{CommentsFileName}.{DoFile.ContentFileNameExtension}";
            var contentFilePath = this.fileSystemAccessor.CombinePath(basePath, fileName);

            this.fileSystemAccessor.WriteAllText(contentFilePath, doContent.ToString());
        }

        private void WriteFileHeader(bool hasAtLeastOneRoster, int maxRosterDepthInQuestionnaire, string commentsFilePath)
        {
            var commentsHeader = GetHeadersList(hasAtLeastOneRoster, maxRosterDepthInQuestionnaire);

            this.csvWriter.WriteData(commentsFilePath, new[] {commentsHeader.ToArray()}, ExportFileSettings.DataFileSeparator.ToString());
        }

        private List<string> GetHeadersList(bool hasAtLeastOneRoster, int maxRosterDepthInQuestionnaire)
        {
            var commentsHeader = new List<string> {"Order", "Originator", "Role", "Date", "Time", "Variable"};

            if (hasAtLeastOneRoster)
                commentsHeader.Add("Roster");

            commentsHeader.Add("interview__id");
            commentsHeader.Add("interview__key");

            for (int i = 1; i <= maxRosterDepthInQuestionnaire; i++)
            {
                commentsHeader.Add($"Id{i}");
            }

            commentsHeader.Add("Comment");
            return commentsHeader;
        }

        private List<string[]> QueryCommentsChunkFromReadSide(
            Expression<Func<InterviewCommentaries, bool>> queryComments,
            int maxRosterDepthInQuestionnaire, bool hasAtLeastOneRoster, Dictionary<string, string> interviewKeysMap)
        {
            var comments = this.interviewCommentariesStorage.Query(
                            _ =>
                                _.Where(queryComments)
                                    .SelectMany(
                                        interviewComments => interviewComments.Commentaries,
                                        (interview, comment) => new { interview.InterviewId, Comments = comment })
                                    .Select(i => new
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
                resultRow.Add(interviewKeysMap[interview.InterviewId]);

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
