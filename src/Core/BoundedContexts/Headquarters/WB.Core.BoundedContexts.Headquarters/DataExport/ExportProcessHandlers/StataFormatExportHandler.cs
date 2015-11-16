using System;
using Microsoft;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportProcess;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;
using IFilebasedExportedDataAccessor = WB.Core.BoundedContexts.Headquarters.DataExport.Accessors.IFilebasedExportedDataAccessor;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers
{
    internal class StataFormatExportHandler : AbstractDataExportHandler
    {
        private readonly ITabularFormatExportService tabularFormatExportService;
        private readonly ITabularDataToExternalStatPackageExportService tabularDataToExternalStatPackageExportService;

        public StataFormatExportHandler(
            IFileSystemAccessor fileSystemAccessor,
            IArchiveUtils archiveUtils,
            InterviewDataExportSettings interviewDataExportSettings,
            ITabularFormatExportService tabularFormatExportService,
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor,
            ITabularDataToExternalStatPackageExportService tabularDataToExternalStatPackageExportService) : base(fileSystemAccessor, archiveUtils, filebasedExportedDataAccessor, interviewDataExportSettings)
        {
            this.tabularFormatExportService = tabularFormatExportService;
            this.tabularDataToExternalStatPackageExportService = tabularDataToExternalStatPackageExportService;
        }

        protected override DataExportFormat Format => DataExportFormat.STATA;

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

            CreateStataDataFilesFromTabularDataFiles(questionnaireIdentity, tabFiles, progress);

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

            CreateStataDataFilesFromTabularDataFiles(questionnaireIdentity, tabFiles, progress);

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
                (sender, donePercent) => progress.Report(donePercent/2);

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

        private void CreateStataDataFilesFromTabularDataFiles(QuestionnaireIdentity questionnaireIdentity, string[] tabDataFiles,
            IProgress<int> progress)
        {
            var exportProgress = new Microsoft.Progress<int>();
            exportProgress.ProgressChanged +=
                (sender, donePercent) => progress.Report(50 + (donePercent/2));

           tabularDataToExternalStatPackageExportService.CreateAndGetStataDataFilesForQuestionnaire(
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