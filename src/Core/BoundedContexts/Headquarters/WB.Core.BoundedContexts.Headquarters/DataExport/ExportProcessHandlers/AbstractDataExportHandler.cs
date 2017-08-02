using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers
{
    abstract class AbstractDataExportHandler : IExportProcessHandler<DataExportProcessDetails>
    {
        protected readonly IFileSystemAccessor fileSystemAccessor;
        private readonly InterviewDataExportSettings interviewDataExportSettings;
        private readonly IFilebasedExportedDataAccessor filebasedExportedDataAccessor;
        private readonly IDataExportProcessesService dataExportProcessesService;
        private readonly ILogger logger;
        private readonly IDataExportFileAccessor dataExportFileAccessor;


        protected AbstractDataExportHandler(
            IFileSystemAccessor fileSystemAccessor,
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor,
            InterviewDataExportSettings interviewDataExportSettings, 
            IDataExportProcessesService dataExportProcessesService,
            IDataExportFileAccessor dataExportFileAccessor,
            ILogger logger)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.interviewDataExportSettings = interviewDataExportSettings;
            this.dataExportProcessesService = dataExportProcessesService;
            this.logger = logger;
            this.dataExportFileAccessor = dataExportFileAccessor;
            this.filebasedExportedDataAccessor = filebasedExportedDataAccessor;
        }

        public void ExportData(DataExportProcessDetails dataExportProcessDetails)
        {
            dataExportProcessDetails.CancellationToken.ThrowIfCancellationRequested();

            var folderPathOfDataByQuestionnaire = this.GetFolderPathOfDataByQuestionnaire(dataExportProcessDetails.Questionnaire);

            string outputFolderByStatus = dataExportProcessDetails.InterviewStatus?.ToString() ?? "All";
            string folderForDataExport = this.fileSystemAccessor.CombinePath(folderPathOfDataByQuestionnaire, outputFolderByStatus);

            this.ClearFolder(folderForDataExport);

            dataExportProcessDetails.CancellationToken.ThrowIfCancellationRequested();

            var exportProgress = new Progress<int>();

            exportProgress.ProgressChanged +=
                (sender, donePercent) => UpdateDataExportProgress(dataExportProcessDetails.NaturalId, donePercent);

            this.ExportDataIntoDirectory(dataExportProcessDetails.Questionnaire, dataExportProcessDetails.InterviewStatus, folderForDataExport, exportProgress, dataExportProcessDetails.CancellationToken);

            dataExportProcessDetails.CancellationToken.ThrowIfCancellationRequested();

            var filesToArchive = this.fileSystemAccessor.GetFilesInDirectory(folderForDataExport);

            dataExportFileAccessor.RecreateExportArchive(filesToArchive, this.GetArchiveNameForData(dataExportProcessDetails.Questionnaire, dataExportProcessDetails.InterviewStatus));

            this.ClearFolder(folderForDataExport);
        }

        protected abstract DataExportFormat Format { get; }

        protected abstract void ExportDataIntoDirectory(QuestionnaireIdentity questionnaireIdentity, InterviewStatus? status, string directoryPath, IProgress<int> progress, CancellationToken cancellationToken);

        private void UpdateDataExportProgress(string dataExportProcessDetailsId, int progressInPercents)
        {
            this.dataExportProcessesService.UpdateDataExportProgress(dataExportProcessDetailsId,
                progressInPercents);
        }

        private string GetArchiveNameForData(QuestionnaireIdentity questionnaireIdentity, InterviewStatus? interviewStatus)
        {
            return this.filebasedExportedDataAccessor.GetArchiveFilePathForExportedData(questionnaireIdentity, Format, interviewStatus);
        }

        private string GetFolderPathOfDataByQuestionnaire(QuestionnaireIdentity questionnaireIdentity)
        {
            var pathToExportedData = fileSystemAccessor.CombinePath(interviewDataExportSettings.DirectoryPath,
                $"TemporaryExportDataFor{this.Format}");

            if (!fileSystemAccessor.IsDirectoryExists(pathToExportedData))
                fileSystemAccessor.CreateDirectory(pathToExportedData);

            return this.fileSystemAccessor.CombinePath(pathToExportedData,
                $"{questionnaireIdentity.QuestionnaireId}_{questionnaireIdentity.Version}");
        }

        private void ClearFolder(string folderName)
        {
            if (this.fileSystemAccessor.IsDirectoryExists(folderName))
                this.fileSystemAccessor.DeleteDirectory(folderName);

            this.fileSystemAccessor.CreateDirectory(folderName);
        }
    }
}