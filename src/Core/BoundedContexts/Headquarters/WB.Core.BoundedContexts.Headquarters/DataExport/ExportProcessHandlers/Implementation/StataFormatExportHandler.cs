using System;
using System.Threading;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.Services.Export;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.Export;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using IFilebasedExportedDataAccessor = WB.Core.BoundedContexts.Headquarters.DataExport.Accessors.IFilebasedExportedDataAccessor;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers.Implementation
{
    [Obsolete("KP-11815")]
    internal class StataFormatExportHandler : TabBasedFormatExportHandler
    {
        private readonly ITabularDataToExternalStatPackageExportService tabularDataToExternalStatPackageExportService;

        public StataFormatExportHandler(IFileSystemAccessor fileSystemAccessor,
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor, 
            InterviewDataExportSettings interviewDataExportSettings, 
            IDataExportProcessesService dataExportProcessesService, 
            ITabularFormatExportService tabularFormatExportService, 
            ITabularDataToExternalStatPackageExportService tabularDataToExternalStatPackageExportService,
            ILogger logger,
            IDataExportFileAccessor dataExportFileAccessor)
            : base(fileSystemAccessor, filebasedExportedDataAccessor, interviewDataExportSettings, dataExportProcessesService, tabularFormatExportService, logger, dataExportFileAccessor)
        {
            this.tabularDataToExternalStatPackageExportService = tabularDataToExternalStatPackageExportService;
        }

        protected override DataExportFormat Format => DataExportFormat.STATA;

        protected override void ExportDataIntoDirectory(ExportSettings settings, IProgress<int> progress,
            CancellationToken cancellationToken)
        {
            var tabFiles = this.CreateTabularDataFiles(settings.QuestionnaireId, settings.InterviewStatus,
                settings.ExportDirectory, progress, cancellationToken, settings.FromDate, settings.ToDate);

            this.CreateStataDataFilesFromTabularDataFiles(settings.QuestionnaireId, tabFiles, progress, cancellationToken);

            this.DeleteTabularDataFiles(tabFiles, cancellationToken);

            this.GenerateDescriptionTxt(settings.QuestionnaireId, settings.ExportDirectory, ExportFileSettings.StataDataFileExtension);
        }

        private void CreateStataDataFilesFromTabularDataFiles(QuestionnaireIdentity questionnaireIdentity, string[] tabDataFiles,
            IProgress<int> progress, CancellationToken cancellationToken)
        {
            var exportProgress = new Progress<int>();
            exportProgress.ProgressChanged +=
                (sender, donePercent) => progress.Report(50 + (donePercent/2));

           tabularDataToExternalStatPackageExportService.CreateAndGetStataDataFilesForQuestionnaire(
                questionnaireIdentity.QuestionnaireId,
                questionnaireIdentity.Version,
                tabDataFiles,
                exportProgress,
                cancellationToken);
        }
    }
}
