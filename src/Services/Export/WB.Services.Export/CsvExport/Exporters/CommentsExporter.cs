using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services;
using WB.Services.Export.Tenant;
using WB.Services.Export.Utils;

namespace WB.Services.Export.CsvExport.Exporters
{
    internal class CommentsExporter : ICommentsExporter
    {
        private readonly InterviewDataExportSettings interviewDataExportSettings;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ICsvWriter csvWriter;
        private readonly string dataFileExtension = "tab";
        public readonly string CommentsFileName = "interview__comments";
        private readonly ITenantApi<IHeadquartersApi> tenantApi;
        private readonly ILogger<CommentsExporter> logger;

        public readonly DoExportFileHeader[] CommentsFileColumns =
        {
            new DoExportFileHeader("Order", "Sequential order of the comment"),
            new DoExportFileHeader("Originator", "Login name of the person leaving the comment"),
            new DoExportFileHeader("Role", "System role of the person leaving the comment"),
            new DoExportFileHeader("Date", "Date when the comment was left"),
            new DoExportFileHeader("Time", "Time when the comment was left"),
            new DoExportFileHeader("Variable", "Variable name for the commented question"),
            new DoExportFileHeader("interview__id", "Unique 32-character long identifier of the interview"),
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
            IOptions<InterviewDataExportSettings> interviewDataExportSettings,
            IFileSystemAccessor fileSystemAccessor,
            ICsvWriter csvWriter,
            ITenantApi<IHeadquartersApi> tenantApi,
            ILogger<CommentsExporter> logger)
        {
            this.interviewDataExportSettings = interviewDataExportSettings.Value;
            this.fileSystemAccessor = fileSystemAccessor;
            this.csvWriter = csvWriter;
            this.tenantApi = tenantApi;
            this.tenantApi = tenantApi;
            this.logger = logger;
        }

        public async Task ExportAsync(QuestionnaireExportStructure questionnaireExportStructure,
            List<Guid> interviewIdsToExport,
            string basePath,
            TenantInfo tenant,
            IProgress<int> progress,
            CancellationToken cancellationToken)
        {
            var batchSize = this.interviewDataExportSettings.MaxRecordsCountPerOneExportQuery;

            string commentsFilePath = Path.Combine(basePath, Path.ChangeExtension(this.CommentsFileName, this.dataFileExtension));
            int maxRosterDepthInQuestionnaire = questionnaireExportStructure.HeaderToLevelMap.Values.Max(x => x.LevelScopeVector.Count);
            bool hasAtLeastOneRoster = questionnaireExportStructure.HeaderToLevelMap.Values.Any(x => x.LevelScopeVector.Count > 0);
            this.WriteFileHeader(hasAtLeastOneRoster, maxRosterDepthInQuestionnaire, commentsFilePath);

            long totalProcessed = 0;
            var stopwatch = Stopwatch.StartNew();

            foreach (var interviewIds in interviewIdsToExport.Batch(batchSize))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var comments = await tenantApi.For(tenant).GetInterviewCommentsBatchAsync(interviewIds.ToArray());

                var rows = ConvertToCsvStrings(comments, maxRosterDepthInQuestionnaire, hasAtLeastOneRoster);
                this.csvWriter.WriteData(commentsFilePath, rows, ExportFileSettings.DataFileSeparator.ToString());
                totalProcessed++;
                progress.Report(totalProcessed.PercentOf(interviewIdsToExport.Count));
            }
            
            stopwatch.Stop();
            this.logger.Log(LogLevel.Information, "Exported all interview comments. Took {0:g} to export {1:N0} interviews", 
                stopwatch.Elapsed, interviewIdsToExport.Count);
            progress.Report(100);
        }

        private List<string[]> ConvertToCsvStrings(List<InterviewComment> comments, int maxRosterDepthInQuestionnaire, bool hasAtLeastOneRoster)
        {
            var result = new List<string[]>();

            foreach (var comment in comments)
            {
                var resultRow = new List<string>
                {
                    comment.CommentSequence.ToString(),
                    comment.OriginatorName,
                    this.GetUserRole(comment.OriginatorRole),
                    comment.Timestamp.ToString("d", CultureInfo.InvariantCulture),
                    comment.Timestamp.ToString("T", CultureInfo.InvariantCulture),
                    comment.Variable
                };

                if (hasAtLeastOneRoster)
                    resultRow.Add(comment.Roster);

                resultRow.Add(comment.InterviewId.ToString("N"));

                for (int i = 0; i < maxRosterDepthInQuestionnaire; i++)
                {
                    resultRow.Add(comment.RosterVector.Length > i ? comment.RosterVector[i].ToString(CultureInfo.InvariantCulture) : "");
                }

                resultRow.Add(comment.Comment);

                result.Add(resultRow.ToArray());
            }
            return result;
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
            var contentFilePath = Path.Combine(basePath, fileName);

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

            for (int i = 1; i <= maxRosterDepthInQuestionnaire; i++)
            {
                commentsHeader.Add($"Id{i}");
            }

            commentsHeader.Add("Comment");
            return commentsHeader;
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