using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Ddi;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Models;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Questionnaire.Services;
using WB.Services.Export.Services;
using WB.Services.Infrastructure.Tenant;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.CsvExport.Implementation
{
    public class TabularFormatExportService : ITabularFormatExportService
    {
        private readonly ILogger<TabularFormatExportService> logger;
        private readonly IInterviewsToExportSource interviewsToExportSource;

        private readonly ICommentsExporter commentsExporter;
        private readonly IInterviewActionsExporter interviewActionsExporter;
        private readonly IDiagnosticsExporter diagnosticsExporter;
        private readonly IQuestionnaireExportStructureFactory exportStructureFactory;
        private readonly IQuestionnaireStorage questionnaireStorage;

        private readonly IProductVersion productVersion;
        private readonly IPdfExporter pdfExporter;
        private readonly IQuestionnaireBackupExporter questionnaireBackupExporter;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IAssignmentActionsExporter assignmentActionsExporter;
        private readonly IInterviewsExporter interviewsExporter;
        private readonly IDdiMetadataFactory ddiMetadataFactory;

        public TabularFormatExportService(
            ILogger<TabularFormatExportService> logger,
            IInterviewsToExportSource interviewsToExportSource,
            IInterviewsExporter interviewsExporter,
            ICommentsExporter commentsExporter,
            IDiagnosticsExporter diagnosticsExporter,
            IInterviewActionsExporter interviewActionsExporter,
            IQuestionnaireExportStructureFactory exportStructureFactory,
            IQuestionnaireStorage questionnaireStorage,
            IProductVersion productVersion, 
            IPdfExporter pdfExporter,
            IFileSystemAccessor fileSystemAccessor,
            IAssignmentActionsExporter assignmentActionsExporter,
            IQuestionnaireBackupExporter questionnaireBackupExporter, 
            IDdiMetadataFactory ddiMetadataFactory)
        {
            this.logger = logger;
            this.interviewsToExportSource = interviewsToExportSource;
            this.interviewsExporter = interviewsExporter;
            this.commentsExporter = commentsExporter;
            this.diagnosticsExporter = diagnosticsExporter;
            this.interviewActionsExporter = interviewActionsExporter;
            this.exportStructureFactory = exportStructureFactory;
            this.questionnaireStorage = questionnaireStorage;
            this.productVersion = productVersion;
            this.pdfExporter = pdfExporter;
            this.fileSystemAccessor = fileSystemAccessor;
            this.assignmentActionsExporter = assignmentActionsExporter;
            this.questionnaireBackupExporter = questionnaireBackupExporter;
            this.ddiMetadataFactory = ddiMetadataFactory;
        }

        public async Task ExportInterviewsInTabularFormatAsync(
            ExportSettings settings,
            string tempPath,
            ExportProgress progress,
            CancellationToken cancellationToken)
        {
            var tenant = settings.Tenant;
            var questionnaireIdentity = settings.QuestionnaireId;
            var status = settings.Status;
            var fromDate = settings.FromDate;
            var toDate = settings.ToDate;

            var questionnaire = await this.questionnaireStorage.GetQuestionnaireAsync(questionnaireIdentity, token: cancellationToken);
            if(questionnaire == null)
                throw new InvalidOperationException("questionnaire must be not null.");
            QuestionnaireExportStructure questionnaireExportStructure = this.exportStructureFactory.CreateQuestionnaireExportStructure(questionnaire);

            var exportInterviewsProgress = new ExportProgress();
            var exportCommentsProgress = new ExportProgress();
            var exportInterviewActionsProgress = new ExportProgress();
            var exportDiagnosticsProgress = new ExportProgress();
            var exportAssignmentActionsProgress = new ExportProgress();

            ProgressAggregator progressAggregator = new ProgressAggregator();
            progressAggregator.Add(exportInterviewsProgress, 0.4);
            progressAggregator.Add(exportCommentsProgress, 0.1);
            progressAggregator.Add(exportInterviewActionsProgress, 0.2);
            progressAggregator.Add(exportDiagnosticsProgress, 0.1);
            progressAggregator.Add(exportAssignmentActionsProgress, 0.2);

            progressAggregator.ProgressChanged += (sender, overallProgress) => progress.Report(overallProgress);

            var interviewsToExport = this.interviewsToExportSource.GetInterviewsToExport(questionnaireIdentity, status, fromDate, toDate);
            var interviewIdsToExport = interviewsToExport.Select(x => x.Id).ToList();

            bool shouldExportAllAssignments = status == null && fromDate == null && toDate == null;
       

            Stopwatch exportWatch = Stopwatch.StartNew();

            // TODO: Make them real parallel
            await this.commentsExporter.ExportAsync(questionnaireExportStructure, interviewIdsToExport, tempPath, tenant, exportCommentsProgress, cancellationToken);
            await this.interviewActionsExporter.ExportAsync(tenant, questionnaireIdentity, interviewIdsToExport, tempPath, exportInterviewActionsProgress, cancellationToken);
            await this.interviewsExporter.ExportAsync(tenant, questionnaireExportStructure, questionnaire, interviewsToExport, tempPath, exportInterviewsProgress, cancellationToken);
            await this.diagnosticsExporter.ExportAsync(interviewIdsToExport, tempPath, tenant,  exportDiagnosticsProgress, cancellationToken);

            if (shouldExportAllAssignments)
            {
                await this.assignmentActionsExporter.ExportAllAsync(tenant, 
                    questionnaireIdentity,
                    tempPath,
                    exportAssignmentActionsProgress,
                    cancellationToken);
            }
            else
            {
                var assignmentIdsToExport = 
                    interviewsToExport.Where(x => x.AssignmentId.HasValue)
                                      .Select(x => x.AssignmentId!.Value)
                                      .Distinct()
                                      .ToList();
                await this.assignmentActionsExporter.ExportAsync(assignmentIdsToExport, 
                    tenant, tempPath,  exportAssignmentActionsProgress, cancellationToken);
            }

            if (settings.IncludeMeta != false)
            {
                await this.pdfExporter.ExportAsync(tenant, questionnaire, tempPath, cancellationToken);
                await this.questionnaireBackupExporter.ExportAsync(tenant, questionnaire, tempPath, cancellationToken);
                await this.ddiMetadataFactory.CreateDDIMetadataFileForQuestionnaireInFolderAsync(tenant,
                    questionnaire.QuestionnaireId, tempPath);
            }

            exportWatch.Stop();

            this.logger.LogInformation("Export with all steps finished for questionnaire {questionnaireIdentity}. " +
                                       "Took {elapsed:c} to export {interviewIds} interviews",
                questionnaireIdentity, exportWatch.Elapsed, interviewIdsToExport.Count
            );
        }
        
        public async Task GenerateDescriptionFileAsync(TenantInfo tenant, QuestionnaireIdentity questionnaireId, string basePath, string dataFilesExtension, CancellationToken cancellationToken)
        {
            var questionnaire = await this.questionnaireStorage.GetQuestionnaireAsync(questionnaireId, token: cancellationToken);
            if (questionnaire == null)
                throw new InvalidOperationException("questionnaire must be not null.");

            QuestionnaireExportStructure questionnaireExportStructure = await this.exportStructureFactory.GetQuestionnaireExportStructureAsync(tenant, questionnaireId);

            var questionnaireUrl = $"https://designer.mysurvey.solutions/questionnaire/details/{questionnaire.PublicKey.ToString("N")}";
            if (questionnaire.Revision > 0)
                questionnaireUrl += $"${questionnaire.Revision}";

            var descriptionBuilder = new StringBuilder();
            descriptionBuilder.AppendLine($"Generated by Survey Solutions export module {this.productVersion} on {DateTime.Today:D}");
            descriptionBuilder.AppendLine($"The data in this download were collected using the Survey Solutions questionnaire \"{questionnaire.Title}\". ");
            descriptionBuilder.AppendLine($"You can open the questionnaire in the Survey Solutions Designer online by that link: {questionnaireUrl}");

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
