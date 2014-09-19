using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ionic.Zip;
using Ionic.Zlib;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport
{
    internal class FileBasedDataExportService : IDataExportService
    {
        private const string ExportedDataFolderName = "ExportedData";
        private const string ExportedFilesFolderName = "ExportedFiles";
        private readonly string pathToExportedData;
        private readonly string pathToExportedFiles;
        private readonly IDataFileExportService dataFileExportService;
        private readonly IEnvironmentContentService environmentContentService;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ILogger logger;
        private readonly IPlainFileRepository plainFileRepository;

        public FileBasedDataExportService(
            IReadSideRepositoryCleanerRegistry cleanerRegistry, 
            string folderPath,
            IDataFileExportService dataFileExportService, 
            IEnvironmentContentService environmentContentService,
            IFileSystemAccessor fileSystemAccessor, ILogger logger, IPlainFileRepository plainFileRepository)
        {
            this.dataFileExportService = dataFileExportService;
            this.environmentContentService = environmentContentService;
            this.fileSystemAccessor = fileSystemAccessor;
            this.logger = logger;
            this.plainFileRepository = plainFileRepository;
            this.pathToExportedData = fileSystemAccessor.CombinePath(folderPath, ExportedDataFolderName);

            if (!fileSystemAccessor.IsDirectoryExists(this.pathToExportedData))
                fileSystemAccessor.CreateDirectory(this.pathToExportedData);

            this.pathToExportedFiles = fileSystemAccessor.CombinePath(folderPath, ExportedFilesFolderName);

            if (!fileSystemAccessor.IsDirectoryExists(this.pathToExportedFiles))
                fileSystemAccessor.CreateDirectory(this.pathToExportedFiles);

            cleanerRegistry.Register(this);
        }

        public void Clear()
        {
            Array.ForEach(this.fileSystemAccessor.GetDirectoriesInDirectory(this.pathToExportedData), (s) => this.fileSystemAccessor.DeleteDirectory(s));
            Array.ForEach(this.fileSystemAccessor.GetFilesInDirectory(this.pathToExportedData), (s) => this.fileSystemAccessor.DeleteFile(s));

            Array.ForEach(this.fileSystemAccessor.GetDirectoriesInDirectory(this.pathToExportedFiles), (s) => this.fileSystemAccessor.DeleteDirectory(s));
            Array.ForEach(this.fileSystemAccessor.GetFilesInDirectory(this.pathToExportedFiles), (s) => this.fileSystemAccessor.DeleteFile(s));
        }

        public string GetFilePathToExportedCompressedData(Guid questionnaireId, long version)
        {
            var dataDirectoryPath = this.GetFolderPathOfDataByQuestionnaire(questionnaireId, version);

            this.ThrowArgumentExceptionIfDataFolderMissing(questionnaireId, version, dataDirectoryPath);

            var archiveFilePath = this.fileSystemAccessor.CombinePath(this.pathToExportedData, string.Format("{0}.zip", this.fileSystemAccessor.GetFileName(dataDirectoryPath)));

            if (this.fileSystemAccessor.IsFileExists(archiveFilePath))
                this.fileSystemAccessor.DeleteFile(archiveFilePath);

            using (var zip = new ZipFile(this.fileSystemAccessor.GetFileName(archiveFilePath)))
            {
                zip.CompressionLevel = CompressionLevel.BestCompression;
                zip.AddDirectory(dataDirectoryPath);
                zip.Save(archiveFilePath);
            }

            return archiveFilePath;
        }

        public string GetFilePathToExportedBinaryData(Guid questionnaireId, long version)
        {
            var fileDirectoryPath = this.GetFolderPathOfFilesByQuestionnaire(questionnaireId, version);

            this.ThrowArgumentExceptionIfFilesFolderMissing(questionnaireId, version, fileDirectoryPath);

            var archiveFilePath = this.fileSystemAccessor.CombinePath(this.pathToExportedData, string.Format("{0}.zip", this.fileSystemAccessor.GetFileName(fileDirectoryPath)));

            if (this.fileSystemAccessor.IsFileExists(archiveFilePath))
                this.fileSystemAccessor.DeleteFile(archiveFilePath);

            using (var zip = new ZipFile(this.fileSystemAccessor.GetFileName(archiveFilePath)))
            {
                zip.CompressionLevel = CompressionLevel.BestCompression;
                zip.AddDirectory(fileDirectoryPath);
                zip.Save(archiveFilePath);
            }

            return archiveFilePath;

        }

        public void CreateExportStructureByTemplate(QuestionnaireExportStructure questionnaireExportStructure)
        {
            this.CreateExportedDataStructure(questionnaireExportStructure);
            this.CreateExportedFileStructure(questionnaireExportStructure);
        }

        private void CreateExportedFileStructure(QuestionnaireExportStructure questionnaireExportStructure)
        {
            var filesFolderForTemplatePath = this.GetFolderPathOfFilesByQuestionnaire(questionnaireExportStructure.QuestionnaireId,
                questionnaireExportStructure.Version);

            if (this.fileSystemAccessor.IsDirectoryExists(filesFolderForTemplatePath))
            {
                string copyPath = this.GetPreviousCopiesOfFilesFolderPath();

                this.logger.Error(string.Format("Directory for export files already exists: {0}. Will be moved to {1}.",
                    filesFolderForTemplatePath, copyPath));

                this.fileSystemAccessor.CopyFileOrDirectory(filesFolderForTemplatePath, copyPath);

                this.logger.Info(string.Format("Existing directory for export files {0} copied to {1}", filesFolderForTemplatePath, copyPath));

                this.fileSystemAccessor.DeleteDirectory(filesFolderForTemplatePath);

                this.logger.Info(string.Format("Existing directory for export files {0} deleted", filesFolderForTemplatePath));
            }

            this.fileSystemAccessor.CreateDirectory(filesFolderForTemplatePath);
        }

        private void CreateExportedDataStructure(QuestionnaireExportStructure questionnaireExportStructure)
        {
            var dataFolderForTemplatePath = this.GetFolderPathOfDataByQuestionnaire(questionnaireExportStructure.QuestionnaireId,
                questionnaireExportStructure.Version);

            if (this.fileSystemAccessor.IsDirectoryExists(dataFolderForTemplatePath))
            {
                string copyPath = this.GetPreviousCopiesFolderPath();

                this.logger.Error(string.Format("Directory for export structure already exists: {0}. Will be moved to {1}.",
                    dataFolderForTemplatePath, copyPath));

                this.fileSystemAccessor.CopyFileOrDirectory(dataFolderForTemplatePath, copyPath);

                this.logger.Info(string.Format("Existing directory for export structure {0} copied to {1}", dataFolderForTemplatePath, copyPath));

                this.fileSystemAccessor.DeleteDirectory(dataFolderForTemplatePath);

                this.logger.Info(string.Format("Existing directory for export structure {0} deleted", dataFolderForTemplatePath));
            }

            this.fileSystemAccessor.CreateDirectory(dataFolderForTemplatePath);


            foreach (var headerStructureForLevel in questionnaireExportStructure.HeaderToLevelMap.Values)
            {
                string levelFileName = headerStructureForLevel.LevelName;

                var interviewExportedDataFileName = this.dataFileExportService.GetInterviewExportedDataFileName(levelFileName);
                var contentOfAdditionalFileName = this.environmentContentService.GetEnvironmentContentFileName(levelFileName);

                this.dataFileExportService.CreateHeader(headerStructureForLevel,
                    this.fileSystemAccessor.CombinePath(dataFolderForTemplatePath, interviewExportedDataFileName));

                this.environmentContentService.CreateContentOfAdditionalFile(headerStructureForLevel, interviewExportedDataFileName,
                    this.fileSystemAccessor.CombinePath(dataFolderForTemplatePath, contentOfAdditionalFileName));
            }

            this.dataFileExportService.CreateHeaderForActionFile(this.fileSystemAccessor.CombinePath(dataFolderForTemplatePath,
                this.dataFileExportService.GetInterviewActionFileName()));
        }

        public void DeleteExportedDataForQuestionnaireVersion(Guid questionnaireId, long questionnaireVersion)
        {
            var dataFolderForTemplatePath = this.GetFolderPathOfDataByQuestionnaire(questionnaireId, questionnaireVersion);
            if (this.fileSystemAccessor.IsDirectoryExists(dataFolderForTemplatePath))
            {
                this.fileSystemAccessor.DeleteDirectory(dataFolderForTemplatePath);
            }

            var filesFolderForTemplatePath = this.GetFolderPathOfFilesByQuestionnaire(questionnaireId, questionnaireVersion);
            if (this.fileSystemAccessor.IsDirectoryExists(filesFolderForTemplatePath))
            {
                this.fileSystemAccessor.DeleteDirectory(filesFolderForTemplatePath);
            }
        }

        public void AddExportedDataByInterview(InterviewDataExportView interviewDataExportView)
        {
            var dataFolderForTemplatePath = this.GetFolderPathOfDataByQuestionnaire(interviewDataExportView.TemplateId, interviewDataExportView.TemplateVersion);

            this.ThrowArgumentExceptionIfDataFolderMissing(interviewDataExportView.TemplateId, interviewDataExportView.TemplateVersion, dataFolderForTemplatePath);
           
            foreach (var interviewDataExportLevelView in interviewDataExportView.Levels)
            {
                string levelFileName = interviewDataExportLevelView.LevelName;

                this.dataFileExportService.AddRecord(interviewDataExportLevelView, this.fileSystemAccessor.CombinePath(dataFolderForTemplatePath, this.dataFileExportService.GetInterviewExportedDataFileName(levelFileName)));
            }

            var filesFolderForTemplatePath = this.GetFolderPathOfFilesByQuestionnaire(interviewDataExportView.TemplateId, interviewDataExportView.TemplateVersion);

            this.ThrowArgumentExceptionIfFilesFolderMissing(interviewDataExportView.TemplateId, interviewDataExportView.TemplateVersion, filesFolderForTemplatePath);

            var filesFolderForInterview = GetFolderPathOfFilesByQuestionnaireForInterview(interviewDataExportView.TemplateId,
                interviewDataExportView.TemplateVersion, interviewDataExportView.InterviewId);

            if (fileSystemAccessor.IsDirectoryExists(filesFolderForInterview))
                fileSystemAccessor.DeleteDirectory(filesFolderForInterview);

            fileSystemAccessor.CreateDirectory(filesFolderForInterview);

            var filesToMove = plainFileRepository.GetBinaryFilesForInterview(interviewDataExportView.InterviewId);

            foreach (var file in filesToMove)
            {
                fileSystemAccessor.WriteAllBytes(fileSystemAccessor.CombinePath(filesFolderForInterview, file.FileName), file.Data);
            }
        }

        public void AddInterviewActions(Guid questionnaireId, long questionnaireVersion, IEnumerable<InterviewActionExportView> actions)
        {
            var dataFolderForTemplatePath = this.GetFolderPathOfDataByQuestionnaire(questionnaireId, questionnaireVersion);

            this.ThrowArgumentExceptionIfDataFolderMissing(questionnaireId, questionnaireVersion, dataFolderForTemplatePath);

            this.dataFileExportService.AddActionRecords(actions, this.fileSystemAccessor.CombinePath(dataFolderForTemplatePath, this.dataFileExportService.GetInterviewActionFileName()));
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

        private string GetPreviousCopiesFolderPath()
        {
            return this.fileSystemAccessor.CombinePath(this.pathToExportedData, string.Format("_prv_{0}", DateTime.Now.Ticks));
        }

        private string GetPreviousCopiesOfFilesFolderPath()
        {
            return this.fileSystemAccessor.CombinePath(this.pathToExportedFiles, string.Format("_prv_{0}", DateTime.Now.Ticks));
        }

        private string GetFolderPathOfDataByQuestionnaire(Guid questionnaireId, long version)
        {
            return this.fileSystemAccessor.CombinePath(this.pathToExportedData, string.Format("exported_data_{0}_{1}", questionnaireId, version));
        }

        private string GetFolderPathOfFilesByQuestionnaire(Guid questionnaireId, long version)
        {
            return this.fileSystemAccessor.CombinePath(this.pathToExportedFiles, string.Format("exported_files_{0}_{1}", questionnaireId, version));
        }

        private string GetFolderPathOfFilesByQuestionnaireForInterview(Guid questionnaireId, long version, Guid interviewId)
        {
            return this.fileSystemAccessor.CombinePath(GetFolderPathOfFilesByQuestionnaire(questionnaireId, version),
                string.Format("interview_{0}", interviewId.FormatGuid()));
        }
    }
}
