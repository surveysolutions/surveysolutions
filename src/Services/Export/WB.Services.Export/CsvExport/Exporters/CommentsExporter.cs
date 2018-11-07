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
using WB.Services.Export.Utils;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.CsvExport.Exporters
{
    public class CommentsExporter : ICommentsExporter
    {
        private readonly IOptions<InterviewDataExportSettings> interviewDataExportSettings;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ICsvWriter csvWriter;
        private readonly string dataFileExtension = "tab";
        public string CommentsFileName => "interview__comments";
        private readonly ITenantApi<IHeadquartersApi> tenantApi;
        private readonly ILogger<CommentsExporter> logger;

        public DoExportFileHeader[] CommentsFileColumns => new []
        {
            CommonHeaderItems.InterviewKey,
            CommonHeaderItems.InterviewId,
            CommonHeaderItems.Roster,
            CommonHeaderItems.Id1,
            CommonHeaderItems.Id2,
            CommonHeaderItems.Id3,
            CommonHeaderItems.Id4,
            new DoExportFileHeader("order", "Sequential order of the comment", ExportValueType.NumericInt),
            new DoExportFileHeader("originator", "Login name of the person leaving the comment", ExportValueType.String),
            new DoExportFileHeader("role", "System role of the person leaving the comment", ExportValueType.String),
            new DoExportFileHeader("date", "Date when the comment was left", ExportValueType.String),
            new DoExportFileHeader("time", "Time when the comment was left", ExportValueType.String),
            new DoExportFileHeader("variable", "Variable name for the commented question", ExportValueType.String),
            new DoExportFileHeader("comment", "Text of the comment", ExportValueType.String)
        };

        public CommentsExporter(
            IOptions<InterviewDataExportSettings> interviewDataExportSettings,
            IFileSystemAccessor fileSystemAccessor,
            ICsvWriter csvWriter,
            ITenantApi<IHeadquartersApi> tenantApi,
            ILogger<CommentsExporter> logger)
        {
            this.interviewDataExportSettings = interviewDataExportSettings;
            this.fileSystemAccessor = fileSystemAccessor;
            this.csvWriter = csvWriter;
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
            var batchSize = this.interviewDataExportSettings.Value.MaxRecordsCountPerOneExportQuery;

            string commentsFilePath = Path.Combine(basePath, Path.ChangeExtension(this.CommentsFileName, this.dataFileExtension));
            int maxRosterDepthInQuestionnaire = questionnaireExportStructure.HeaderToLevelMap.Values.Max(x => x.LevelScopeVector.Count);
            bool hasAtLeastOneRoster = questionnaireExportStructure.HeaderToLevelMap.Values.Any(x => x.LevelScopeVector.Count > 0);
            this.WriteFileHeader(hasAtLeastOneRoster, maxRosterDepthInQuestionnaire, commentsFilePath);

            long totalProcessed = 0;
            var stopwatch = Stopwatch.StartNew();

            var headquartersApi = tenantApi.For(tenant);

            foreach (var interviewIds in interviewIdsToExport.Batch(batchSize))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var comments = await headquartersApi.GetInterviewCommentsBatchAsync(interviewIds.ToArray());

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
                    comment.InterviewKey,
                    comment.InterviewId.ToString("N")
                };

                if (hasAtLeastOneRoster)
                    resultRow.Add(comment.Roster);

                for (int i = 0; i < maxRosterDepthInQuestionnaire; i++)
                {
                    resultRow.Add(comment.RosterVector.Length > i ? comment.RosterVector[i].ToString(CultureInfo.InvariantCulture) : "");
                }
                
                resultRow.Add(comment.Variable);
                resultRow.Add(comment.CommentSequence.ToString());
                resultRow.Add(comment.Timestamp.ToString("d", CultureInfo.InvariantCulture));
                resultRow.Add(comment.Timestamp.ToString("T", CultureInfo.InvariantCulture));
                resultRow.Add(comment.OriginatorName);
                resultRow.Add(this.GetUserRole(comment.OriginatorRole));
                resultRow.Add(comment.Comment);

                result.Add(resultRow.ToArray());
            }
            return result;
        }

        public void ExportDoFile(QuestionnaireExportStructure questionnaireExportStructure, string basePath)
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
                    if (exportFileHeader.AddCaption)
                        doContent.AppendCaptionLabelToVariableMatching(exportFileHeader.Title, exportFileHeader.Description);
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
            var commentsHeader = new List<string> { "interview__key", "interview__id"};

            if (hasAtLeastOneRoster)
                commentsHeader.Add("roster");
            
            for (int i = 1; i <= maxRosterDepthInQuestionnaire; i++)
            {
                commentsHeader.Add($"id{i}");
            }

            commentsHeader.Add("variable");
            commentsHeader.Add("order");
            commentsHeader.Add("date");
            commentsHeader.Add("time");
            commentsHeader.Add("originator");
            commentsHeader.Add("role");
            
            commentsHeader.Add("comment");
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
