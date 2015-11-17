using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers
{
    internal class SpssFormatExportHandler : AbstractDataExportHandler
    {
        private readonly ITabularFormatExportService tabularFormatExportService;
        private readonly ITabularDataToExternalStatPackageExportService tabularDataToExternalStatPackageExportService;

        public SpssFormatExportHandler(
            IFileSystemAccessor fileSystemAccessor,
            IArchiveUtils archiveUtils,
            InterviewDataExportSettings interviewDataExportSettings,
            ITabularFormatExportService tabularFormatExportService,
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor,
            ITabularDataToExternalStatPackageExportService tabularDataToExternalStatPackageExportService,
            IDataExportProcessesService dataExportProcessesService
            ) : base(fileSystemAccessor, archiveUtils, filebasedExportedDataAccessor, interviewDataExportSettings, dataExportProcessesService)
        {
            this.tabularFormatExportService = tabularFormatExportService;
            this.tabularDataToExternalStatPackageExportService = tabularDataToExternalStatPackageExportService;
        }

        protected override DataExportFormat Format => DataExportFormat.SPSS;

        protected override void ExportAllDataIntoDirectory(
            QuestionnaireIdentity questionnaireIdentity,
            string directoryPath,
            IProgress<int> progress)
        {
            var tabFiles = CreateTabularDataFiles(
               questionnaireIdentity: questionnaireIdentity,
               directoryPath: directoryPath,
               exportApprovedDataOnly: false,
               progress: progress);

            this.CreateSpssDataFilesFromTabularDataFiles(questionnaireIdentity, tabFiles, progress);

            DeleteTabularDataFiles(tabFiles);
        }

        protected override void ExportApprovedDataIntoDirectory(
            QuestionnaireIdentity questionnaireIdentity,
            string directoryPath,
            IProgress<int> progress)
        {
            var tabFiles = CreateTabularDataFiles(
               questionnaireIdentity: questionnaireIdentity,
               directoryPath: directoryPath,
               exportApprovedDataOnly: true,
               progress: progress);

            this.CreateSpssDataFilesFromTabularDataFiles(questionnaireIdentity, tabFiles, progress);

            DeleteTabularDataFiles(tabFiles);
        }

        private string[] CreateTabularDataFiles(
               QuestionnaireIdentity questionnaireIdentity,
               string directoryPath,
               bool exportApprovedDataOnly,
               IProgress<int> progress)
        {
            var exportProgress = new Microsoft.Progress<int>();

            exportProgress.ProgressChanged +=
                (sender, donePercent) => progress.Report(donePercent / 2);

            if (exportApprovedDataOnly)
            {
                tabularFormatExportService.ExportApprovedInterviewsInTabularFormat(questionnaireIdentity, directoryPath,
                    exportProgress);
            }
            else
            {
                tabularFormatExportService.ExportInterviewsInTabularFormat(questionnaireIdentity, directoryPath,
                    exportProgress);
            }

            return fileSystemAccessor.GetFilesInDirectory(directoryPath);
        }

        private void CreateSpssDataFilesFromTabularDataFiles(QuestionnaireIdentity questionnaireIdentity, string[] tabDataFiles,
            IProgress<int> progress)
        {
            var exportProgress = new Microsoft.Progress<int>();
            exportProgress.ProgressChanged +=
                (sender, donePercent) => progress.Report(50 + (donePercent / 2));

            tabularDataToExternalStatPackageExportService.CreateAndGetSpssDataFilesForQuestionnaire(
                questionnaireIdentity.QuestionnaireId,
                questionnaireIdentity.Version,
                tabDataFiles,
                exportProgress);
        }

        private void DeleteTabularDataFiles(string[] tabDataFiles)
        {
            foreach (var tabDataFile in tabDataFiles)
            {
                fileSystemAccessor.DeleteFile(tabDataFile);
            }
        }
    }
}