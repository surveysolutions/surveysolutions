﻿using System;
using System.Threading;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.Services.Export;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers
{
    internal class SpssFormatExportHandler : TabBasedFormatExportHandler
    {
        private readonly ITabularDataToExternalStatPackageExportService tabularDataToExternalStatPackageExportService;

        public SpssFormatExportHandler(IFileSystemAccessor fileSystemAccessor,
            IArchiveUtils archiveUtils,
            InterviewDataExportSettings interviewDataExportSettings, 
            ITabularFormatExportService tabularFormatExportService, 
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor, 
            ITabularDataToExternalStatPackageExportService tabularDataToExternalStatPackageExportService, 
            IDataExportProcessesService dataExportProcessesService,
            ILogger logger)
            : base(fileSystemAccessor, archiveUtils, filebasedExportedDataAccessor, interviewDataExportSettings, dataExportProcessesService, tabularFormatExportService, logger)
        {
            this.tabularDataToExternalStatPackageExportService = tabularDataToExternalStatPackageExportService;
        }

        protected override DataExportFormat Format => DataExportFormat.SPSS;

        protected override void ExportDataIntoDirectory(QuestionnaireIdentity questionnaireIdentity, InterviewStatus? status, string directoryPath, IProgress<int> progress, CancellationToken cancellationToken)
        {
            var tabFiles = this.CreateTabularDataFiles(questionnaireIdentity, status, directoryPath, progress, cancellationToken);

            this.CreateSpssDataFilesFromTabularDataFiles(questionnaireIdentity, tabFiles, progress, cancellationToken);

            this.DeleteTabularDataFiles(tabFiles, cancellationToken);
        }

        private void CreateSpssDataFilesFromTabularDataFiles(QuestionnaireIdentity questionnaireIdentity, string[] tabDataFiles,
            IProgress<int> progress, CancellationToken cancellationToken)
        {
            var exportProgress = new Microsoft.Progress<int>();
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