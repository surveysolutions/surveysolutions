using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.DescriptionGenerator;
using WB.Services.Export.ExportProcessHandlers;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Questionnaire.Services;
using WB.Services.Export.Services;
using WB.Services.Export.Tenant;
using WB.Services.Export.Utils;

namespace WB.Services.Export.CsvExport.Implementation
{
    public class TabularFormatExportService : ITabularFormatExportService
    {
        //private readonly string dataFileExtension = "tab";

        private readonly ILogger<TabularFormatExportService> logger;
        private readonly ITenantApi<IHeadquartersApi> tenantApi;

        private readonly ICommentsExporter commentsExporter;
        private readonly IInterviewActionsExporter interviewActionsExporter;
        private readonly IDiagnosticsExporter diagnosticsExporter;
        private readonly IQuestionnaireExportStructureFactory exportStructureFactory;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IDescriptionGenerator descriptionGenerator;
        private readonly IEnvironmentContentService environmentContentService;

        private readonly IProductVersion productVersion;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IInterviewsExporter interviewsExporter;

        public TabularFormatExportService(ILogger<TabularFormatExportService> logger,
            ITenantApi<IHeadquartersApi> tenantApi,
            IInterviewsExporter interviewsExporter,
            ICommentsExporter commentsExporter,
            IDiagnosticsExporter diagnosticsExporter,
            IInterviewActionsExporter interviewActionsExporter,
            IQuestionnaireExportStructureFactory exportStructureFactory,
            IQuestionnaireStorage questionnaireStorage,
            IDescriptionGenerator descriptionGenerator,
            IEnvironmentContentService environmentContentService, IProductVersion productVersion, IFileSystemAccessor fileSystemAccessor)
        {
            this.logger = logger;
            this.tenantApi = tenantApi;
            this.interviewsExporter = interviewsExporter;
            this.commentsExporter = commentsExporter;
            this.diagnosticsExporter = diagnosticsExporter;
            this.interviewActionsExporter = interviewActionsExporter;
            this.exportStructureFactory = exportStructureFactory;
            this.questionnaireStorage = questionnaireStorage;
            this.descriptionGenerator = descriptionGenerator;
            this.environmentContentService = environmentContentService;
            this.productVersion = productVersion;
            this.fileSystemAccessor = fileSystemAccessor;
        }

        public async Task ExportInterviewsInTabularFormat(
            ExportSettings settings,
            IProgress<int> progress,
            CancellationToken cancellationToken)
        {
            var tenant = settings.Tenant;
            var questionnaireIdentity = settings.QuestionnaireId;
            var basePath = settings.ExportTempDirectory;
            var status = settings.InterviewStatus;
            var fromDate = settings.FromDate;
            var toDate = settings.ToDate;

            var questionnaire = await this.questionnaireStorage.GetQuestionnaireAsync(tenant, questionnaireIdentity);

            QuestionnaireExportStructure questionnaireExportStructure = this.exportStructureFactory.GetQuestionnaireExportStructure(tenant, questionnaire);

            var exportInterviewsProgress = new Progress<int>();
            var exportCommentsProgress = new Progress<int>();
            var exportInterviewActionsProgress = new Progress<int>();

            ProgressAggregator proggressAggregator = new ProgressAggregator();
            proggressAggregator.Add(exportInterviewsProgress, 0.8);
            proggressAggregator.Add(exportCommentsProgress, 0.1);
            proggressAggregator.Add(exportInterviewActionsProgress, 0.1);
            proggressAggregator.ProgressChanged += (sender, overallProgress) => progress.Report(overallProgress);

            var api = this.tenantApi.For(tenant);
            var interviewsToExport = await api.GetInterviewsToExportAsync(questionnaireIdentity, status, fromDate, toDate);
            var interviewIdsToExport = interviewsToExport.Select(x => x.Id).ToList();

            Stopwatch exportWatch = Stopwatch.StartNew();

            await Task.WhenAll(
                this.commentsExporter.ExportAsync(questionnaireExportStructure, interviewIdsToExport, basePath, tenant, exportCommentsProgress, cancellationToken),
                this.interviewActionsExporter.ExportAsync(tenant, questionnaireIdentity, interviewIdsToExport, basePath, exportInterviewActionsProgress),
                this.interviewsExporter.ExportAsync(tenant, questionnaireExportStructure, questionnaire, interviewsToExport, basePath, exportInterviewsProgress, cancellationToken),
                this.diagnosticsExporter.ExportAsync(interviewIdsToExport, basePath, tenant, exportInterviewsProgress, cancellationToken)
            );

            exportWatch.Stop();
            
            this.logger.Log(LogLevel.Information, $"Export with all steps finished for questionnaire {questionnaireIdentity}. " +
                             $"Took {exportWatch.Elapsed:c} to export {interviewIdsToExport.Count} interviews");
        }
        
        public void GenerateDescriptionFile(TenantInfo tenant, QuestionnaireId questionnaireId, string basePath, string dataFilesExtension)
        {
            QuestionnaireExportStructure questionnaireExportStructure = this.exportStructureFactory.GetQuestionnaireExportStructure(tenant, questionnaireId);

            var descriptionBuilder = new StringBuilder();
            descriptionBuilder.AppendLine(
                $"Exported from Survey Solutions Headquarters {this.productVersion} on {DateTime.Today:D}");

            foreach (var level in questionnaireExportStructure.HeaderToLevelMap.Values)
            {
                string fileName = $"{level.LevelName}{dataFilesExtension}";
                var variables = level.HeaderItems.Values.Select(question => question.VariableName);

                descriptionBuilder.AppendLine();
                descriptionBuilder.AppendLine(fileName);
                descriptionBuilder.AppendLine(string.Join(", ", variables));
            }

            this.fileSystemAccessor.WriteAllText(
                Path.Combine(basePath, "export__readme.txt"),
                descriptionBuilder.ToString());
        }
    }
}
