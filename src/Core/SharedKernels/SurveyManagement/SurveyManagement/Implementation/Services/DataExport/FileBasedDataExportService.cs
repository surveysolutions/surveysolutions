using System;
using System.Collections.Generic;
using System.Linq;
using Ionic.Zip;
using Ionic.Zlib;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.SurveyManagement.Services;
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

        public FileBasedDataExportService(
            IReadSideRepositoryCleanerRegistry cleanerRegistry, 
            string folderPath,
            IDataFileExportService dataFileExportService, 
            IEnvironmentContentService environmentContentService, 
            IFileSystemAccessor fileSystemAccessor)
        {
            this.dataFileExportService = dataFileExportService;
            this.environmentContentService = environmentContentService;
            this.fileSystemAccessor = fileSystemAccessor;
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
                throw new InterviewDataExportException(
                    string.Format("export structure for questionnaire with id'{0}' and version '{1}' was build before",
                        questionnaireExportStructure.QuestionnaireId, questionnaireExportStructure.Version));
            }

            this.fileSystemAccessor.CreateDirectory(dataFolderForTemplatePath);

            var cleanedFileNamesForLevels =
              this.CreateCleanedFileNamesForLevels(questionnaireExportStructure.HeaderToLevelMap.Values.ToDictionary(h => h.LevelId, h => h.LevelName));

            foreach (var headerStructureForLevel in questionnaireExportStructure.HeaderToLevelMap.Values)
            {
                string levelFileName = cleanedFileNamesForLevels[headerStructureForLevel.LevelId];

                var interviewExportedDataFileName = this.dataFileExportService.GetInterviewExportedDataFileName(levelFileName);
                var contentOfAdditionalFileName = this.environmentContentService.GetEnvironmentContentFileName(levelFileName);

                this.dataFileExportService.CreateHeader(headerStructureForLevel,
                    this.fileSystemAccessor.CombinePath(dataFolderForTemplatePath, interviewExportedDataFileName));

                this.environmentContentService.CreateContentOfAdditionalFile(headerStructureForLevel, interviewExportedDataFileName,
                    this.fileSystemAccessor.CombinePath(dataFolderForTemplatePath, contentOfAdditionalFileName));
            }
        }

        public void AddExportedDataByInterview(InterviewDataExportView interviewDataExportView)
        {
            var dataFolderForTemplatePath = this.GetFolderPathOfDataByQuestionnaire(interviewDataExportView.TemplateId, interviewDataExportView.TemplateVersion);

            this.ThrowArgumentExceptionIfDataFolderMissing(interviewDataExportView.TemplateId, interviewDataExportView.TemplateVersion, dataFolderForTemplatePath);

            var cleanedFileNamesForLevels =
                this.CreateCleanedFileNamesForLevels(interviewDataExportView.Levels.ToDictionary(l => l.LevelId, l => l.LevelName));

            foreach (var interviewDataExportLevelView in interviewDataExportView.Levels)
            {
                string levelFileName = cleanedFileNamesForLevels[interviewDataExportLevelView.LevelId];

                this.dataFileExportService.AddRecord(interviewDataExportLevelView, this.fileSystemAccessor.CombinePath(dataFolderForTemplatePath, this.dataFileExportService.GetInterviewExportedDataFileName(levelFileName)));
            }
        }

        private Dictionary<Guid, string> CreateCleanedFileNamesForLevels(Dictionary<Guid, string> allegedLevelNames)
        {
            var result = new Dictionary<Guid, string>();
            var createdFileNames = new HashSet<string>();
            foreach (var allegedLevelName in allegedLevelNames)
            {
                string levelFileName = this.CreateValidFileName(allegedLevelName.Value, createdFileNames);
                createdFileNames.Add(levelFileName);
                result.Add(allegedLevelName.Key,levelFileName);
            }
            return result;
        }

        private void ThrowArgumentExceptionIfDataFolderMissing(Guid questionnaireId, long version, string dataDirectoryPath)
        {
            if (!this.fileSystemAccessor.IsDirectoryExists(dataDirectoryPath))
                throw new InterviewDataExportException(
                    string.Format("data files are absent for questionnaire with id '{0}' and version '{1}'",
                        questionnaireId, version));
        }

        private string GetFolderPathOfDataByQuestionnaire(Guid questionnaireId, long version)
        {
            return this.fileSystemAccessor.CombinePath(this.path, string.Format("exported_data_{0}_{1}", questionnaireId, version));
        }

        private string CreateValidFileName(string name, HashSet<string> createdFileNames, int i = 0)
        {
            string fileNameWithoutInvalidFileNameChars = this.fileSystemAccessor.MakeValidFileName(name);
            var fileNameShortened = new string(fileNameWithoutInvalidFileNameChars.Take(250).ToArray());
            string fileNameWithNumber = string.Concat(fileNameShortened,
                i == 0 ? (object) string.Empty : i).ToLower();

            return !createdFileNames.Contains(fileNameWithNumber)
                ? fileNameWithNumber
                : this.CreateValidFileName(name, createdFileNames, i + 1);
        }
    }
}
