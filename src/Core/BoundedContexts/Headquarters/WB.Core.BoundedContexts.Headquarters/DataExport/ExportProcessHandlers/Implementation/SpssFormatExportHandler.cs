using System;
using System.Threading;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.Services.Export;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.Export;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers.Implementation
{
    [Obsolete("KP-11815")]
    internal class SpssFormatExportHandler : TabBasedFormatExportHandler
    {
        private readonly ITabularDataToExternalStatPackageExportService tabularDataToExternalStatPackageExportService;

        public SpssFormatExportHandler(IFileSystemAccessor fileSystemAccessor,
            InterviewDataExportSettings interviewDataExportSettings, 
            ITabularFormatExportService tabularFormatExportService, 
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor, 
            ITabularDataToExternalStatPackageExportService tabularDataToExternalStatPackageExportService, 
            IDataExportProcessesService dataExportProcessesService,
            ILogger logger,
            IDataExportFileAccessor dataExportFileAccessor)
            : base(fileSystemAccessor, filebasedExportedDataAccessor, interviewDataExportSettings, dataExportProcessesService, tabularFormatExportService, logger, dataExportFileAccessor)
        {
            this.tabularDataToExternalStatPackageExportService = tabularDataToExternalStatPackageExportService;
        }

        protected override DataExportFormat Format => DataExportFormat.SPSS;

        protected override void ExportDataIntoDirectory(ExportSettings settings, IProgress<int> progress,
            CancellationToken cancellationToken)
        {
            var tabFiles = this.CreateTabularDataFiles(settings.QuestionnaireId, settings.InterviewStatus,
                settings.ExportDirectory, progress, cancellationToken, settings.FromDate, settings.ToDate);

            this.CreateSpssDataFilesFromTabularDataFiles(settings.QuestionnaireId, tabFiles, progress, cancellationToken);

            this.DeleteTabularDataFiles(tabFiles, cancellationToken);

            this.GenerateDescriptionTxt(settings.QuestionnaireId, settings.ExportDirectory, ExportFileSettings.SpssDataFileExtension);
        }

        private void CreateSpssDataFilesFromTabularDataFiles(QuestionnaireIdentity questionnaireIdentity, string[] tabDataFiles,
            IProgress<int> progress, CancellationToken cancellationToken)
        {
            var exportProgress = new Progress<int>();
            exportProgress.ProgressChanged +=
                (sender, donePercent) => progress.Report(50 + (donePercent / 2));

            tabularDataToExternalStatPackageExportService.CreateAndGetSpssDataFilesForQuestionnaire(
                questionnaireIdentity.QuestionnaireId,
                questionnaireIdentity.Version,
                tabDataFiles,
                exportProgress,
                cancellationToken);
        }
    }
}
