using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport
{
    internal class FilebasedExportedDataAccessor : IFilebasedExportedDataAccessor
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IArchiveUtils archiveUtils;
        private readonly IDataExportService dataExportService;
        private readonly IEnvironmentContentService environmentContentService;

        private readonly ILogger logger;

        private const string ExportedDataFolderName = "ExportedData";
        private const string ExportedFilesFolderName = "ExportedFiles";
        private readonly string pathToExportedData;
        private readonly string pathToExportedFiles;

        public FilebasedExportedDataAccessor(IFileSystemAccessor fileSystemAccessor,
            string folderPath, IDataExportService dataExportService, IEnvironmentContentService environmentContentService, ILogger logger, IArchiveUtils archiveUtils)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.dataExportService = dataExportService;
            this.environmentContentService = environmentContentService;
            this.logger = logger;
            this.archiveUtils = archiveUtils;
            this.pathToExportedData = fileSystemAccessor.CombinePath(folderPath, ExportedDataFolderName);

            if (!fileSystemAccessor.IsDirectoryExists(this.pathToExportedData))
                fileSystemAccessor.CreateDirectory(this.pathToExportedData);

            this.pathToExportedFiles = fileSystemAccessor.CombinePath(folderPath, ExportedFilesFolderName);

            if (!fileSystemAccessor.IsDirectoryExists(this.pathToExportedFiles))
                fileSystemAccessor.CreateDirectory(this.pathToExportedFiles);
        }

        public string GetFolderPathOfDataByQuestionnaire(Guid questionnaireId, long version)
        {
            var result = this.GetFolderPathOfDataByQuestionnaireImpl(questionnaireId, version);

            this.ThrowArgumentExceptionIfDataFolderMissing(questionnaireId, version, result);

            return result;
        }

        private string GetFolderPathOfDataByQuestionnaireImpl(Guid questionnaireId, long version)
        {
            return this.fileSystemAccessor.CombinePath(this.pathToExportedData,
                string.Format("exported_data_{0}_{1}", questionnaireId, version));
        }

        public string GetFolderPathOfFilesByQuestionnaire(Guid questionnaireId, long version)
        {
            var result = this.GetFolderPathOfFilesByQuestionnaireImpl( questionnaireId, version);

            this.ThrowArgumentExceptionIfFilesFolderMissing(questionnaireId, version, result);

            return result;
        }

        private string GetFolderPathOfFilesByQuestionnaireImpl(Guid questionnaireId, long version)
        {
            return this.fileSystemAccessor.CombinePath(this.pathToExportedFiles,
                string.Format("exported_files_{0}_{1}", questionnaireId, version));
        }

        private void ThrowArgumentExceptionIfDataFolderMissing(Guid questionnaireId, long version, string dataDirectoryPath)
        {
            if (!this.fileSystemAccessor.IsDirectoryExists(dataDirectoryPath))
                throw new InterviewDataExportException(
                    string.Format("data files are absent for questionnaire with id '{0}' and version '{1}'",
                        questionnaireId, version));
        }

        private void ThrowArgumentExceptionIfFilesFolderMissing(Guid templateId, long templateVersion, string filesFolderForTemplatePath)
        {
            if (!this.fileSystemAccessor.IsDirectoryExists(filesFolderForTemplatePath))
                throw new InterviewDataExportException(
                    string.Format("files folder is absent for questionnaire with id '{0}' and version '{1}'",
                        templateId, templateVersion));
        }

        public string CreateExportDataFolder(Guid questionnaireId, long version)
        {
            var folderPath = this.GetFolderPathOfDataByQuestionnaireImpl(questionnaireId, version);
            this.CreateExportFolder(folderPath);
            return folderPath;
        }

        public void CleanExportDataFolder()
        {
            Array.ForEach(this.fileSystemAccessor.GetDirectoriesInDirectory(this.PathToExportedData), (s) => this.fileSystemAccessor.DeleteDirectory(s));
            Array.ForEach(this.fileSystemAccessor.GetFilesInDirectory(this.PathToExportedData), (s) => this.fileSystemAccessor.DeleteFile(s));
        }

        public string CreateExportFileFolder(Guid questionnaireId, long version)
        {
            var folderPath = this.GetFolderPathOfFilesByQuestionnaireImpl(questionnaireId, version);
            this.CreateExportFolder(folderPath);
            return folderPath;
        }

        public void CleanExportFileFolder()
        {
            Array.ForEach(this.fileSystemAccessor.GetDirectoriesInDirectory(this.PathToExportedFiles), (s) => this.fileSystemAccessor.DeleteDirectory(s));
            Array.ForEach(this.fileSystemAccessor.GetFilesInDirectory(this.PathToExportedFiles), (s) => this.fileSystemAccessor.DeleteFile(s));
        }

        private void CreateExportFolder(string folderPath)
        {
            var dataFolderForTemplatePath = folderPath;

            if (this.fileSystemAccessor.IsDirectoryExists(dataFolderForTemplatePath))
            {
                string copyPath = this.PreviousCopiesFolderPath;

                this.logger.Error(string.Format("Directory for export structure already exists: {0}. Will be moved to {1}.",
                    dataFolderForTemplatePath, copyPath));

                this.fileSystemAccessor.CopyFileOrDirectory(dataFolderForTemplatePath, copyPath);

                this.logger.Info(string.Format("Existing directory for export structure {0} copied to {1}", dataFolderForTemplatePath, copyPath));

                this.fileSystemAccessor.DeleteDirectory(dataFolderForTemplatePath);

                this.logger.Info(string.Format("Existing directory for export structure {0} deleted", dataFolderForTemplatePath));
            }

            this.fileSystemAccessor.CreateDirectory(dataFolderForTemplatePath);
        }

        public string GetFilePathToExportedCompressedData(Guid questionnaireId, long version)
        {
            var dataDirectoryPath = this.GetFolderPathOfDataByQuestionnaire(questionnaireId, version);

            var archiveFilePath = this.fileSystemAccessor.CombinePath(this.PathToExportedData, string.Format("{0}.zip", this.fileSystemAccessor.GetFileName(dataDirectoryPath)));

            if (this.fileSystemAccessor.IsFileExists(archiveFilePath))
                this.fileSystemAccessor.DeleteFile(archiveFilePath);

            var filesToArchive = new List<string>();

            filesToArchive.AddRange(this.dataExportService.GetDataFilesForQuestionnaire(questionnaireId, version, dataDirectoryPath));
            filesToArchive.AddRange(this.environmentContentService.GetContentFilesForQuestionnaire(questionnaireId, version, dataDirectoryPath));

            archiveUtils.ZipFiles(filesToArchive, new string[0], archiveFilePath);

            return archiveFilePath;
        }

        public string GetFilePathToExportedApprovedCompressedData(Guid questionnaireId, long version)
        {
            var dataDirectoryPath = this.GetFolderPathOfDataByQuestionnaire(questionnaireId, version);

            var archiveFilePath = this.fileSystemAccessor.CombinePath(this.PathToExportedData, string.Format("{0}_Approved.zip", this.fileSystemAccessor.GetFileName(dataDirectoryPath)));

            if (this.fileSystemAccessor.IsFileExists(archiveFilePath))
                this.fileSystemAccessor.DeleteFile(archiveFilePath);

            var filesToArchive = new List<string>();

            filesToArchive.AddRange(this.dataExportService.GetDataFilesForQuestionnaireByInterviewsInApprovedState(questionnaireId, version, dataDirectoryPath));
            filesToArchive.AddRange(this.environmentContentService.GetContentFilesForQuestionnaire(questionnaireId, version, dataDirectoryPath));

            archiveUtils.ZipFiles(filesToArchive, new string[0], archiveFilePath);
          
            return archiveFilePath;
        }

        public string GetFilePathToExportedBinaryData(Guid questionnaireId, long version)
        {
            var fileDirectoryPath = this.GetFolderPathOfFilesByQuestionnaire(questionnaireId, version);

            var archiveFilePath = this.fileSystemAccessor.CombinePath(this.PathToExportedData, string.Format("{0}.zip", this.fileSystemAccessor.GetFileName(fileDirectoryPath)));

            if (this.fileSystemAccessor.IsFileExists(archiveFilePath))
                this.fileSystemAccessor.DeleteFile(archiveFilePath);

            archiveUtils.ZipFiles(new string[0], new[] { fileDirectoryPath }, archiveFilePath);

            return archiveFilePath;
        }

        public string GetFolderPathOfFilesByQuestionnaireForInterview(Guid questionnaireId, long version, Guid interviewId)
        {
            return this.fileSystemAccessor.CombinePath(this.GetFolderPathOfFilesByQuestionnaire(questionnaireId, version),
                string.Format("interview_{0}", interviewId.FormatGuid()));
        }

        protected string PathToExportedData { get { return pathToExportedData; } }

        protected string PathToExportedFiles { get { return pathToExportedFiles; } }

        protected string PreviousCopiesOfFilesFolderPath
        {
            get { return this.fileSystemAccessor.CombinePath(this.pathToExportedFiles, string.Format("_prv_{0}", DateTime.Now.Ticks)); }
        }

        protected string PreviousCopiesFolderPath
        {
            get { return this.fileSystemAccessor.CombinePath(this.pathToExportedData, string.Format("_prv_{0}", DateTime.Now.Ticks)); }
        }
    }
}
