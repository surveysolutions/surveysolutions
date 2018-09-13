using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview.Exporters;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services;
using WB.Services.Export.Utils;

namespace WB.Services.Export.Interview
{
    internal class ReadSideToTabularFormatExportService : IInterviewsExporter
    {
        private readonly string dataFileExtension = "tab";

        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ICsvWriter csvWriter;
        private readonly ILogger<ReadSideToTabularFormatExportService> logger;
        private readonly IHeadquartersApi headquartersApi;

        private readonly ICommentsExporter commentsExporter;
        private readonly IInterviewActionsExporter interviewActionsExporter;
        private readonly IInterviewsExporter interviewsExporter;
        private readonly IDiagnosticsExporter diagnosticsExporter;
        private readonly IQuestionnaireExportStructureFactory exportStructureFactory;

        private readonly InterviewDataExportSettings exportSettings;
        private readonly IProductVersion productVersion;

        public ReadSideToTabularFormatExportService(IFileSystemAccessor fileSystemAccessor,
            ICsvWriter csvWriter, 
            ILogger<ReadSideToTabularFormatExportService> logger,
            IOptions<InterviewDataExportSettings> exportSettings, 
            IHeadquartersApi headquartersApi,
            IProductVersion productVersion,
            IInterviewsExporter interviewsExporter,
            ICommentsExporter commentsExporter,
            IInterviewActionsExporter interviewActionsExporter,
            IDiagnosticsExporter diagnosticsExporter,
            IQuestionnaireExportStructureFactory exportStructureFactory)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.csvWriter = csvWriter;
            this.logger = logger;
            this.headquartersApi = headquartersApi;
            this.exportSettings = exportSettings.Value;
            this.productVersion = productVersion;
            this.interviewsExporter = interviewsExporter;
            this.commentsExporter = commentsExporter;
            this.interviewActionsExporter = interviewActionsExporter;
            this.diagnosticsExporter = diagnosticsExporter;
            this.exportStructureFactory = exportStructureFactory;
        }

        public async Task ExportInterviewsInTabularFormat(
            string tenantId,
            string questionnaireIdentity,
            InterviewStatus? status,
            string basePath,
            IProgress<int> progress, 
            CancellationToken cancellationToken, 
            DateTime? fromDate,
            DateTime? toDate)
        {
            QuestionnaireExportStructure questionnaireExportStructure = await this.GetQuestionnaireExportStructure(questionnaireIdentity, tenantId);

            var exportInterviewsProgress = new Progress<int>();
            var exportCommentsProgress = new Progress<int>();
            var exportInterviewActionsProgress = new Progress<int>();

            ProgressAggregator proggressAggregator = new ProgressAggregator();
            proggressAggregator.Add(exportInterviewsProgress, 0.8);
            proggressAggregator.Add(exportCommentsProgress, 0.1);
            proggressAggregator.Add(exportInterviewActionsProgress, 0.1);

            proggressAggregator.ProgressChanged += (sender, overallProgress) => progress.Report(overallProgress);

            var interviewsToExport = await headquartersApi.GetInterviewsToExportAsync(questionnaireIdentity, status, fromDate, toDate, tenantId, cancellationToken);
            var interviewIdsToExport = interviewsToExport.Select(x => x.Id).ToList();

            Stopwatch exportWatch = Stopwatch.StartNew();

            await this.commentsExporter.Export(questionnaireExportStructure, interviewIdsToExport, basePath, tenantId, exportCommentsProgress, cancellationToken);
            //this.interviewActionsExporter.Export(questionnaireIdentity, interviewIdsToExport, basePath, exportInterviewActionsProgress);
            //this.interviewsExporter.Export(questionnaireExportStructure, interviewsToExport, basePath, exportInterviewsProgress, cancellationToken);
            //this.diagnosticsExporter.Export(interviewIdsToExport, basePath, exportInterviewsProgress, cancellationToken);

            exportWatch.Stop();

            this.logger.Log(LogLevel.Information, $"Export with all steps finished for questionnaire {questionnaireIdentity}. " +
                             $"Took {exportWatch.Elapsed:c} to export {interviewIdsToExport.Count} interviews");
        }

        private Task<QuestionnaireExportStructure> GetQuestionnaireExportStructure(string questionnaireId, string tenantId)
            => this.exportStructureFactory.GetQuestionnaireExportStructure(questionnaireId, tenantId);
    }
}
