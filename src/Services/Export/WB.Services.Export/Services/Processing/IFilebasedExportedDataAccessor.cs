using System;
using Microsoft.Extensions.Options;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Services.Processing
{
    public interface IFilebasedExportedDataAccessor
    {
        string GetArchiveFilePathForExportedData(string archiveName,
            DataExportFormat format,
            InterviewStatus? status = null, DateTime? fromDate = null, DateTime? toDate = null);

        string GetExportDirectory();
        void MoveExportArchive(string tempArchivePath, string archiveName);
    }

    class FilebasedExportedDataAccessor : IFilebasedExportedDataAccessor
    {
        private const string ExportedDataFolderName = "ExportedData";
        private readonly IExportFileNameService exportFileNameService;

        private readonly IOptions<InterviewDataExportSettings> interviewDataExportSettings;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private string pathToExportedData;

        public FilebasedExportedDataAccessor(IExportFileNameService exportFileNameService, IFileSystemAccessor fileSystemAccessor, IOptions<InterviewDataExportSettings> interviewDataExportSettings)
        {
            this.exportFileNameService = exportFileNameService;
            this.fileSystemAccessor = fileSystemAccessor;
            this.interviewDataExportSettings = interviewDataExportSettings;
            this.pathToExportedData = fileSystemAccessor.CombinePath(interviewDataExportSettings.Value.DirectoryPath, ExportedDataFolderName);

            if (!fileSystemAccessor.IsDirectoryExists(this.pathToExportedData))
                fileSystemAccessor.CreateDirectory(this.pathToExportedData);
        }

        public string GetArchiveFilePathForExportedData(string archiveName, DataExportFormat format,
            InterviewStatus? status = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            fromDate = format == DataExportFormat.Binary ? null : fromDate;
            toDate = format == DataExportFormat.Binary ? null : toDate;

            var statusSuffix = status != null && format != DataExportFormat.Binary ? status.ToString() : "All";

            return this.exportFileNameService.GetFileNameForTabByQuestionnaire(
                archiveName, this.pathToExportedData, format, statusSuffix, fromDate, toDate);
        }

        public string GetExportDirectory()
        {
            throw new NotImplementedException();
        }

        public void MoveExportArchive(string tempArchivePath, string archiveName)
        {
            throw new NotImplementedException();
        }
    }
}
