using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.Export;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services.Exporters
{
    public class DiagnosticsExporter
    {
        private readonly InterviewDataExportSettings interviewDataExportSettings;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ICsvWriter csvWriter;
        private readonly ILogger logger;
        private readonly IInterviewDiagnosticsFactory diagnosticsFactory;
        private readonly ITransactionManagerProvider transactionManager;

        private readonly string dataFileExtension = "tab";
        private readonly string diagnosticsFileName = "interview__diagnostics";

        private readonly DoExportFileHeader[] diagnosticsFileColumns =
        {
            new DoExportFileHeader("Interview_key", "Unique 32-character long identifier of the interview"),
            new DoExportFileHeader("interview_status", "Last status of interview"),
            new DoExportFileHeader("responsible", "Last responsible person"),
            new DoExportFileHeader("n_of_Interviewers", "Number of interviewers who worked on this interview"),
            new DoExportFileHeader("n_rejections_by_supervisor", "How many times this interview was rejected by supervisors"),
            new DoExportFileHeader("n_rejections_by_hq", "How many times this interview was rejected by HQ"),
            new DoExportFileHeader("n_quetions_valid", "Number of valid questions"),
            new DoExportFileHeader("n_quetions_errors", "Number of questions with errors"),
            new DoExportFileHeader("n_quetions_unanswered", "Number of unanswered questions"),
            new DoExportFileHeader("n_quetions_comments", "Number of questions with comments"),
            new DoExportFileHeader("interview_duration", "active time it took to complete the interview"),
        };

        public DiagnosticsExporter(InterviewDataExportSettings interviewDataExportSettings,
            IFileSystemAccessor fileSystemAccessor,
            ICsvWriter csvWriter,
            ILogger logger,
            IInterviewDiagnosticsFactory diagnosticsFactory,
            ITransactionManagerProvider transactionManager)
        {
            this.interviewDataExportSettings = interviewDataExportSettings;
            this.fileSystemAccessor = fileSystemAccessor;
            this.csvWriter = csvWriter;
            this.logger = logger;
            this.diagnosticsFactory = diagnosticsFactory;
            this.transactionManager = transactionManager;
        }

        public void Export(QuestionnaireExportStructure questionnaireExportStructure,
            List<Guid> interviewIdsToExport,
            string basePath,
            IProgress<int> progress, 
            CancellationToken cancellationToken)
        {
            var batchSize = this.interviewDataExportSettings.MaxRecordsCountPerOneExportQuery;

            string diagnosticsFilePath = this.fileSystemAccessor.CombinePath(basePath, Path.ChangeExtension(this.diagnosticsFileName, this.dataFileExtension));
            this.WriteFileHeader(diagnosticsFilePath);

            long totalProcessed = 0;
            var stopwatch = Stopwatch.StartNew();
            var etaHelper = new EtaHelper(interviewIdsToExport.Count, batchSize, trackingStopwatch: stopwatch);

            foreach (var interviewsChunk in interviewIdsToExport.Batch(batchSize))
            {
                var interviewIds = interviewsChunk.ToList();
                var diagnosticsRecords = this.transactionManager.GetTransactionManager().ExecuteInQueryTransaction(
                                                () => this.QueryDiagnosticsChunkFromReadSide(interviewIds));

                this.csvWriter.WriteData(diagnosticsFilePath, diagnosticsRecords, ExportFileSettings.DataFileSeparator.ToString());
                totalProcessed += interviewIds.Count;

                etaHelper.AddProgress(interviewIds.Count);

                progress.Report(totalProcessed.PercentOf(interviewIdsToExport.Count));
                this.logger.Debug($"Exported batch of interview diagnostics. {etaHelper} " +
                                  $"Total interview diagnostics processed {totalProcessed:N0} out of {interviewIdsToExport.Count:N0}");
            }
            stopwatch.Stop();
            this.logger.Info($"Exported all interview diagnostics. Took {stopwatch.Elapsed:g} to export {interviewIdsToExport.Count:N0} interviews");
            progress.Report(100);
        }

        private void WriteFileHeader(string commentsFilePath)
        {
            var commentsHeader = diagnosticsFileColumns.Select(h => h.Title).ToArray();

            this.csvWriter.WriteData(commentsFilePath, new[] { commentsHeader }, ExportFileSettings.DataFileSeparator.ToString());
        }

        private List<string[]> QueryDiagnosticsChunkFromReadSide(IEnumerable<Guid> interviewIds)
        {
            List<string[]> interviewsStringData = new List<string[]>();

            var interviews = this.diagnosticsFactory.GetByBatchIds(interviewIds);

            foreach (var interview in interviews)
            {
                interviewsStringData.Add(new string[]
                {
                    interview.InterviewKey,
                    interview.Status.ToString(),
                    interview.ResponsibleName,
                    interview.NumberOfInterviewers.ToString(),
                    interview.NumberRejectionsBySupervisor.ToString(),
                    interview.NumberRejectionsByHq.ToString(),
                    interview.NumberValidQuetions.ToString(),
                    interview.NumberInvalidQuetions.ToString(),
                    interview.NumberUnansweredQuetions.ToString(),
                    interview.NumberCommentedQuetions.ToString(),
                    interview.InterviewDuration?.ToString(),
                });
            }
            return interviewsStringData;
        }
    }
}
