using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport
{
    internal class FilebaseExportRouteService : IFilebaseExportRouteService
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private const string ExportedDataFolderName = "ExportedData";
        private const string ExportedFilesFolderName = "ExportedFiles";
        private readonly string pathToExportedData;
        private readonly string pathToExportedFiles;

        public FilebaseExportRouteService(IFileSystemAccessor fileSystemAccessor,
            string folderPath)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.pathToExportedData = fileSystemAccessor.CombinePath(folderPath, ExportedDataFolderName);

            if (!fileSystemAccessor.IsDirectoryExists(this.pathToExportedData))
                fileSystemAccessor.CreateDirectory(this.pathToExportedData);

            this.pathToExportedFiles = fileSystemAccessor.CombinePath(folderPath, ExportedFilesFolderName);

            if (!fileSystemAccessor.IsDirectoryExists(this.pathToExportedFiles))
                fileSystemAccessor.CreateDirectory(this.pathToExportedFiles);
        }

        public string GetFolderPathOfDataByQuestionnaire(Guid questionnaireId, long version)
        {
            return this.fileSystemAccessor.CombinePath(this.pathToExportedData, string.Format("exported_data_{0}_{1}", questionnaireId, version));
        }

        public string GetFolderPathOfFilesByQuestionnaire(Guid questionnaireId, long version)
        {
            return this.fileSystemAccessor.CombinePath(this.pathToExportedFiles, string.Format("exported_files_{0}_{1}", questionnaireId, version));
        }

        public string GetFolderPathOfFilesByQuestionnaireForInterview(Guid questionnaireId, long version, Guid interviewId)
        {
            return this.fileSystemAccessor.CombinePath(GetFolderPathOfFilesByQuestionnaire(questionnaireId, version),
                string.Format("interview_{0}", interviewId.FormatGuid()));
        }

        public string PathToExportedData { get { return pathToExportedData; } }

        public string PathToExportedFiles { get { return pathToExportedFiles; } }

        public string PreviousCopiesOfFilesFolderPath
        {
            get { return this.fileSystemAccessor.CombinePath(this.pathToExportedFiles, string.Format("_prv_{0}", DateTime.Now.Ticks)); }
        }

        public string PreviousCopiesFolderPath
        {
            get { return this.fileSystemAccessor.CombinePath(this.pathToExportedData, string.Format("_prv_{0}", DateTime.Now.Ticks)); }
        }

        public string ExtensionOfExportedDataFile { get { return "tab"; } }
        public string SeparatorOfExportedDataFile { get { return "\t "; } }
    }
}
