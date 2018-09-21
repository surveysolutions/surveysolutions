using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services;
using WB.Services.Export.Tenant;
using WB.Services.Export.Utils;

namespace WB.Services.Export.CsvExport.Exporters
{
    public class DiagnosticsExporter : IDiagnosticsExporter
    {
        private readonly IOptions<InterviewDataExportSettings> interviewDataExportSettings;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ICsvWriter csvWriter;
        //private readonly ILogger logger;
        private readonly ITenantApi<IHeadquartersApi> tenantApi;
        //private readonly ITransactionManagerProvider transactionManager;

        private readonly string dataFileExtension = "tab";
        public readonly string DiagnosticsFileName = "interview__diagnostics";

        public readonly DoExportFileHeader[] DiagnosticsFileColumns =
        {
            new DoExportFileHeader("interview__id", "Unique 32-character long identifier of the interview"),
            new DoExportFileHeader("interview__key", "Short identifier of the interview"),
            new DoExportFileHeader("interview_status", "Last status of interview"),
            new DoExportFileHeader("responsible", "Last responsible person"),
            new DoExportFileHeader("n_of_Interviewers", "Number of interviewers who worked on this interview"),
            new DoExportFileHeader("n_rejections_by_supervisor", "How many times this interview was rejected by supervisors"),
            new DoExportFileHeader("n_rejections_by_hq", "How many times this interview was rejected by HQ"),
            //new DoExportFileHeader("n_questions_valid", "Number of valid questions"),
            new DoExportFileHeader("n_entities_errors", "Number of questions and static texts with errors"),
            //new DoExportFileHeader("n_questions_unanswered", "Number of unanswered questions"),
            new DoExportFileHeader("n_questions_comments", "Number of questions with comments"),
            new DoExportFileHeader("interview_duration", "Active time it took to complete the interview"),
        };

        public DiagnosticsExporter(
            IOptions<InterviewDataExportSettings> interviewDataExportSettings,
            IFileSystemAccessor fileSystemAccessor,
            ICsvWriter csvWriter,
            ITenantApi<IHeadquartersApi> tenantApi)
        {
            this.interviewDataExportSettings = interviewDataExportSettings;
            this.fileSystemAccessor = fileSystemAccessor;
            this.csvWriter = csvWriter;
           // this.logger = logger;
            this.tenantApi = tenantApi;
        }

        public async Task ExportAsync(List<Guid> interviewIdsToExport, string basePath, TenantInfo tenant, Progress<int> exportInterviewsProgress,
            CancellationToken cancellationToken)
        {
            var batchSize = this.interviewDataExportSettings.Value.MaxRecordsCountPerOneExportQuery;

            string diagnosticsFilePath = Path.Combine(basePath, Path.ChangeExtension(this.DiagnosticsFileName, this.dataFileExtension));
            this.WriteFileHeader(diagnosticsFilePath);

            long totalProcessed = 0;
            var stopwatch = Stopwatch.StartNew();
            //            var etaHelper = new EtaHelper(interviewIdsToExport.Count, batchSize, trackingStopwatch: stopwatch);
            var api = this.tenantApi.For(tenant);

            var data = new List<string[]>(batchSize);

            foreach (var interview in interviewIdsToExport.Batch(batchSize))
            {
                var diagInfos = await api.GetInterviewDiagnosticsInfoBatchAsync(interview.ToArray());

                foreach (var diagInfo in diagInfos)
                {
                    data.Add(new string[]
                    {
                        diagInfo.InterviewId.ToString(),
                        diagInfo.InterviewKey,
                        diagInfo.Status.ToString(),
                        diagInfo.ResponsibleName,
                        diagInfo.NumberOfInterviewers.ToString(),
                        diagInfo.NumberRejectionsBySupervisor.ToString(),
                        diagInfo.NumberRejectionsByHq.ToString(),
                        //diagInfoew.NumberValidQuestions.ToString(),
                        diagInfo.NumberInvalidEntities.ToString(),
                        //diagInfoew.NumberUnansweredQuestions.ToString(),
                        diagInfo.NumberCommentedQuestions.ToString(),
                        diagInfo.InterviewDuration != null
                            ? new TimeSpan(diagInfo.InterviewDuration.Value).ToString("c", CultureInfo.InvariantCulture)
                            : string.Empty,
                    });
                }

                this.csvWriter.WriteData(diagnosticsFilePath, data, ExportFileSettings.DataFileSeparator.ToString());

                totalProcessed++;

                // etaHelper.AddProgress(interviewIds.Count);

                //progress.Report(totalProcessed.PercentOf(interviewIdsToExport.Count));
                //this.logger.Debug($"Exported batch of interview diagnostics. {etaHelper} " +
                //                  $"Total interview diagnostics processed {totalProcessed:N0} out of {interviewIdsToExport.Count:N0}");
            }
            stopwatch.Stop();
            //this.logger.Info($"Exported all interview diagnostics. Took {stopwatch.Elapsed:g} to export {interviewIdsToExport.Count:N0} interviews");
            //progress.Report(100);
        }

        public void ExportDoFile(QuestionnaireExportStructure questionnaireExportStructure, string basePath)
        {
            var doContent = new DoFile();

            doContent.BuildInsheet(Path.ChangeExtension(this.DiagnosticsFileName, this.dataFileExtension));
            doContent.AppendLine();

            foreach (var exportFileHeader in DiagnosticsFileColumns)
            {
                if (exportFileHeader.AddCapture)
                    doContent.AppendCaptureLabelToVariableMatching(exportFileHeader.Title, exportFileHeader.Description);
                else
                    doContent.AppendLabelToVariableMatching(exportFileHeader.Title, exportFileHeader.Description);
            }

            var fileName = $"{DiagnosticsFileName}.{DoFile.ContentFileNameExtension}";
            var contentFilePath = Path.Combine(basePath, fileName);

            this.fileSystemAccessor.WriteAllText(contentFilePath, doContent.ToString());
        }


        private void WriteFileHeader(string commentsFilePath)
        {
            var diagnosticsHeader = DiagnosticsFileColumns.Select(h => h.Title).ToArray();

            this.csvWriter.WriteData(commentsFilePath, new[] { diagnosticsHeader }, ExportFileSettings.DataFileSeparator.ToString());
        }

    }
}
