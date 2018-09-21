using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WB.Services.Export.CsvExport;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services.Processing;
using WB.Services.Export.Tenant;

namespace WB.Services.Export.ExportProcessHandlers.Implementation
{
    internal class TabularFormatDataExportHandler : AbstractDataExportHandler
    {
        private readonly ITabularFormatExportService tabularFormatExportService;
        private readonly IEnvironmentContentService environmentContentService;
        private readonly IQuestionnaireExportStructureFactory questionnaireExportStructureStorage;

        public TabularFormatDataExportHandler(
            IFileSystemAccessor fileSystemAccessor,
            IOptions<InterviewDataExportSettings> interviewDataExportSettings,
            ITabularFormatExportService tabularFormatExportService,
            IEnvironmentContentService environmentContentService,
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor,
            IDataExportProcessesService dataExportProcessesService,
            ILogger<TabularFormatDataExportHandler> logger,
            IQuestionnaireExportStructureFactory questionnaireExportStructureStorage,
            IDataExportFileAccessor dataExportFileAccessor) :
            base(fileSystemAccessor, filebasedExportedDataAccessor, interviewDataExportSettings, dataExportProcessesService, dataExportFileAccessor)
        {
            this.tabularFormatExportService = tabularFormatExportService;
            this.environmentContentService = environmentContentService;
            this.questionnaireExportStructureStorage = questionnaireExportStructureStorage;
        }

        protected override DataExportFormat Format => DataExportFormat.Tabular;

        protected override void ExportDataIntoDirectory(ExportSettings settings, 
            IProgress<int> progress, CancellationToken cancellationToken)
        {
            this.GenerateDescriptionTxt(settings.Tenant, settings.QuestionnaireId, settings.ExportTempDirectory);

            this.tabularFormatExportService.ExportInterviewsInTabularFormat(settings, progress, cancellationToken).Wait(cancellationToken);

            this.CreateDoFilesForQuestionnaire(settings.Tenant, settings.QuestionnaireId, settings.ExportTempDirectory, cancellationToken);
        }

        private void GenerateDescriptionTxt(TenantInfo tenant, QuestionnaireId questionnaireIdentity, string directoryPath)
            => this.tabularFormatExportService.GenerateDescriptionFile(tenant, questionnaireIdentity, directoryPath, ExportFileSettings.TabDataFileExtension);

        private void CreateDoFilesForQuestionnaire(TenantInfo tenant, QuestionnaireId questionnaireIdentity, string directoryPath, CancellationToken cancellationToken)
        {

            var questionnaireExportStructure = this.questionnaireExportStructureStorage
                .GetQuestionnaireExportStructure(tenant, questionnaireIdentity);

            this.environmentContentService.CreateEnvironmentFiles(questionnaireExportStructure, directoryPath, cancellationToken);

        }
    }
}
