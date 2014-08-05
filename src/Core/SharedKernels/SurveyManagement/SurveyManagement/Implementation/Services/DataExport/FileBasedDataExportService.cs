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
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport
{
    internal class FileBasedDataExportService : IDataExportService
    {
        private const string FolderName = "ExportedData";
        private readonly string path;
        private readonly IDataFileExportService dataFileExportService;
        private readonly IEnvironmentContentService environmentContentService;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ILogger logger;

        public FileBasedDataExportService(
            IReadSideRepositoryCleanerRegistry cleanerRegistry, 
            string folderPath,
            IDataFileExportService dataFileExportService, 
            IEnvironmentContentService environmentContentService,
            IFileSystemAccessor fileSystemAccessor, ILogger logger)
        {
            this.dataFileExportService = dataFileExportService;
            this.environmentContentService = environmentContentService;
            this.fileSystemAccessor = fileSystemAccessor;
            this.logger = logger;
            this.path = fileSystemAccessor.CombinePath(folderPath, FolderName);

            if (!fileSystemAccessor.IsDirectoryExists(this.path))
                fileSystemAccessor.CreateDirectory(this.path);

            cleanerRegistry.Register(this);
        }

        public void Clear()
        {
            Array.ForEach(this.fileSystemAccessor.GetDirectoriesInDirectory(this.path), (s) => this.fileSystemAccessor.DeleteDirectory(s));
            Array.ForEach(this.fileSystemAccessor.GetFilesInDirectory(this.path), (s) => this.fileSystemAccessor.DeleteFile(s));
        }

        public string GetFilePathToExportedCompressedData(Guid questionnaireId, long version)
        {
            var dataDirectoryPath = this.GetFolderPathOfDataByQuestionnaire(questionnaireId, version);

            this.ThrowArgumentExceptionIfDataFolderMissing(questionnaireId, version, dataDirectoryPath);

            var archiveFilePath = this.fileSystemAccessor.CombinePath(this.path, string.Format("{0}.zip", this.fileSystemAccessor.GetFileName(dataDirectoryPath)));

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

        public void CreateExportedDataStructureByTemplate(QuestionnaireExportStructure questionnaireExportStructure)
        {
            var dataFolderForTemplatePath = this.GetFolderPathOfDataByQuestionnaire(questionnaireExportStructure.QuestionnaireId, questionnaireExportStructure.Version); 

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

            this.dataFileExportService.CreateHeaderForActionFile(this.fileSystemAccessor.CombinePath(dataFolderForTemplatePath, this.dataFileExportService.GetInterviewActionFileName()));
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

        private string GetPreviousCopiesFolderPath()
        {
            return this.fileSystemAccessor.CombinePath(this.path, string.Format("_prv_{0}", DateTime.Now.Ticks));
        }

        private string GetFolderPathOfDataByQuestionnaire(Guid questionnaireId, long version)
        {
            return this.fileSystemAccessor.CombinePath(this.path, string.Format("exported_data_{0}_{1}", questionnaireId, version));
        }
    }
}
