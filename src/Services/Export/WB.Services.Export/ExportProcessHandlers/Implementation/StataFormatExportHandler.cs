//using System;
//using System.Threading;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
//using WB.Services.Export.CsvExport;
//using WB.Services.Export.Infrastructure;
//using WB.Services.Export.Interview;
//using WB.Services.Export.Questionnaire;
//using WB.Services.Export.Services.Processing;

//namespace WB.Services.Export.ExportProcessHandlers.Implementation
//{
//    internal class StataFormatExportHandler : TabBasedFormatExportHandler
//    {
//        private readonly ITabularDataToExternalStatPackageExportService tabularDataToExternalStatPackageExportService;

//        public StataFormatExportHandler(IFileSystemAccessor fileSystemAccessor,
//            IFilebasedExportedDataAccessor filebasedExportedDataAccessor, 
//            IOptions<InterviewDataExportSettings> interviewDataExportSettings, 
//            IDataExportProcessesService dataExportProcessesService, 
//            ITabularFormatExportService tabularFormatExportService, 
//            ITabularDataToExternalStatPackageExportService tabularDataToExternalStatPackageExportService,
//            ILogger<StataFormatExportHandler> logger,
//            IDataExportFileAccessor dataExportFileAccessor)
//            : base(fileSystemAccessor, filebasedExportedDataAccessor, interviewDataExportSettings, dataExportProcessesService, tabularFormatExportService, logger, dataExportFileAccessor)
//        {
//            this.tabularDataToExternalStatPackageExportService = tabularDataToExternalStatPackageExportService;
//        }

//        protected override DataExportFormat Format => DataExportFormat.STATA;

//        protected override void ExportDataIntoDirectory(ExportSettings settings, IProgress<int> progress,
//            CancellationToken cancellationToken)
//        {
//            var tabFiles = this.CreateTabularDataFiles(settings.QuestionnaireId, settings.InterviewStatus,
//                settings.ExportDirectory, progress, cancellationToken, settings.FromDate, settings.ToDate);

//            this.CreateStataDataFilesFromTabularDataFiles(settings.QuestionnaireId, tabFiles, progress, cancellationToken);

//            this.DeleteTabularDataFiles(tabFiles, cancellationToken);

//            this.GenerateDescriptionTxt(settings.QuestionnaireId, settings.ExportDirectory, ExportFileSettings.StataDataFileExtension);
//        }

//        private void CreateStataDataFilesFromTabularDataFiles(QuestionnaireId questionnaireIdentity, string[] tabDataFiles,
//            IProgress<int> progress, CancellationToken cancellationToken)
//        {
//            var exportProgress = new Progress<int>();
//            exportProgress.ProgressChanged +=
//                (sender, donePercent) => progress.Report(50 + (donePercent/2));

//           tabularDataToExternalStatPackageExportService.CreateAndGetStataDataFilesForQuestionnaire(
//                questionnaireIdentity.QuestionnaireId,
//                questionnaireIdentity.Version,
//                tabDataFiles,
//                exportProgress,
//                cancellationToken);
//        }
//    }
//}
