using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.Ddi;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Accessors
{
    internal class FilebasedExportedDataAccessor : IFilebasedExportedDataAccessor
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private const string ExportedDataFolderName = "ExportedData";
        private readonly string pathToExportedData;

        public FilebasedExportedDataAccessor(
            IFileSystemAccessor fileSystemAccessor,
            InterviewDataExportSettings interviewDataExportSettings)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.pathToExportedData = fileSystemAccessor.CombinePath(interviewDataExportSettings.DirectoryPath, ExportedDataFolderName);

            if (!fileSystemAccessor.IsDirectoryExists(this.pathToExportedData))
                fileSystemAccessor.CreateDirectory(this.pathToExportedData);
        }

        public string GetArchiveFilePathForExportedData(QuestionnaireIdentity questionnaireId, DataExportFormat format)
        {
            var archiveName = $"{questionnaireId.QuestionnaireId}_{questionnaireId.Version}_{format}_All.zip";

            return this.fileSystemAccessor.CombinePath(this.pathToExportedData, archiveName);
        }

        public string GetArchiveFilePathForExportedApprovedData(QuestionnaireIdentity questionnaireId, DataExportFormat format)
        {
            var archiveName = $"{questionnaireId.QuestionnaireId}_{questionnaireId.Version}_{format}_App.zip";
            return this.fileSystemAccessor.CombinePath(this.pathToExportedData, archiveName);
        }
    }
}
