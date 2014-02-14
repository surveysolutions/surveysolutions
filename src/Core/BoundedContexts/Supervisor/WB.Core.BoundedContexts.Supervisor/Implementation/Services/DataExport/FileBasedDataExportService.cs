using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Ionic.Zip;
using Ionic.Zlib;
using Main.Core.View;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Supervisor.Implementation.Services.DataExport
{
    internal class FileBasedDataExportService : IDataExportService
    {
        private const string FolderName = "ExportedData";
        private readonly string path;
        private readonly IInterviewExportService interviewExportService;
        private readonly IEnvironmentContentService environmentContentService;
        private readonly IFileSystemAccessor fileSystemAccessor;

        public FileBasedDataExportService(
            IReadSideRepositoryCleanerRegistry cleanerRegistry, 
            string folderPath,
            IInterviewExportService interviewExportService, 
            IEnvironmentContentService environmentContentService, 
            IFileSystemAccessor fileSystemAccessor)
        {
            this.interviewExportService = interviewExportService;
            this.environmentContentService = environmentContentService;
            this.fileSystemAccessor = fileSystemAccessor;
            this.path = fileSystemAccessor.CombinePath(folderPath, FolderName);

            if (!fileSystemAccessor.IsDirectoryExists(path))
                fileSystemAccessor.CreateDirectory(path);

            cleanerRegistry.Register(this);
        }

        public void Clear()
        {
            Array.ForEach(this.fileSystemAccessor.GetDirectoriesInDirectory(this.path), (s) => this.fileSystemAccessor.DeleteDirectory(s));
            Array.ForEach(this.fileSystemAccessor.GetFilesInDirectory(this.path), (s) => this.fileSystemAccessor.DeleteFile(s));
        }

        public string GetFilePathToExportedCompressedData(Guid questionnaireId, long version)
        {
            var dataDirectoryPath = GetFolderPathOfDataByQuestionnaire(questionnaireId, version);

            ThrowArgumentExceptionIfDataFolderMissing(questionnaireId, version, dataDirectoryPath);

            var archiveFilePath = fileSystemAccessor.CombinePath(path, string.Format("{0}.zip", fileSystemAccessor.GetFileName(dataDirectoryPath)));

            if (fileSystemAccessor.IsFileExists(archiveFilePath))
                fileSystemAccessor.DeleteFile(archiveFilePath);

            using (var zip = new ZipFile(fileSystemAccessor.GetFileName(archiveFilePath)))
            {
                zip.CompressionLevel = CompressionLevel.BestCompression;
                zip.AddDirectory(dataDirectoryPath);
                zip.Save(archiveFilePath);
            }

            return archiveFilePath;
        }

        public void CreateExportedDataStructureByTemplate(QuestionnaireExportStructure questionnaireExportStructure)
        {
            var dataFolderForTemplatePath = GetFolderPathOfDataByQuestionnaire(questionnaireExportStructure.QuestionnaireId, questionnaireExportStructure.Version);

            if (fileSystemAccessor.IsDirectoryExists(dataFolderForTemplatePath))
            {
                throw new InterviewDataExportException(
                    string.Format("export structure for questionnaire with id'{0}' and version '{1}' was build before",
                        questionnaireExportStructure.QuestionnaireId, questionnaireExportStructure.Version));
            }

            fileSystemAccessor.CreateDirectory(dataFolderForTemplatePath);

            var cleanedFileNamesForLevels =
              CreateCleanedFileNamesForLevels(questionnaireExportStructure.HeaderToLevelMap.Values.ToDictionary(h => h.LevelId, h => h.LevelName));

            foreach (var headerStructureForLevel in questionnaireExportStructure.HeaderToLevelMap.Values)
            {
                string levelFileName = cleanedFileNamesForLevels[headerStructureForLevel.LevelId];

                var interviewExportedDataFileName = interviewExportService.GetInterviewExportedDataFileName(levelFileName);
                var contentOfAdditionalFileName = environmentContentService.GetEnvironmentContentFileName(levelFileName);

                this.interviewExportService.CreateHeader(headerStructureForLevel,
                    fileSystemAccessor.CombinePath(dataFolderForTemplatePath, interviewExportedDataFileName));

                this.environmentContentService.CreateContentOfAdditionalFile(headerStructureForLevel, interviewExportedDataFileName,
                    fileSystemAccessor.CombinePath(dataFolderForTemplatePath, contentOfAdditionalFileName));
            }
        }

        public void AddExportedDataByInterview(InterviewDataExportView interviewDataExportView)
        {
            var dataFolderForTemplatePath = GetFolderPathOfDataByQuestionnaire(interviewDataExportView.TemplateId, interviewDataExportView.TemplateVersion);

            ThrowArgumentExceptionIfDataFolderMissing(interviewDataExportView.TemplateId, interviewDataExportView.TemplateVersion, dataFolderForTemplatePath);

            var cleanedFileNamesForLevels =
                CreateCleanedFileNamesForLevels(interviewDataExportView.Levels.ToDictionary(l => l.LevelId, l => l.LevelName));

            foreach (var interviewDataExportLevelView in interviewDataExportView.Levels)
            {
                string levelFileName = cleanedFileNamesForLevels[interviewDataExportLevelView.LevelId];

                this.interviewExportService.AddRecord(interviewDataExportLevelView, fileSystemAccessor.CombinePath(dataFolderForTemplatePath, this.interviewExportService.GetInterviewExportedDataFileName(levelFileName)));
            }
        }

        private Dictionary<Guid, string> CreateCleanedFileNamesForLevels(Dictionary<Guid, string> allegedLevelNames)
        {
            var result = new Dictionary<Guid, string>();
              var createdFileNames = new HashSet<string>();
            foreach (var allegedLevelName in allegedLevelNames)
            {
                string levelFileName = CreateValidFileName(allegedLevelName.Value, createdFileNames);
                createdFileNames.Add(levelFileName);
                result.Add(allegedLevelName.Key,levelFileName);
            }
            return result;
        }

        private void ThrowArgumentExceptionIfDataFolderMissing(Guid questionnaireId, long version, string dataDirectoryPath)
        {
            if (!fileSystemAccessor.IsDirectoryExists(dataDirectoryPath))
                throw new InterviewDataExportException(
                    string.Format("data files are absent for questionnaire with id '{0}' and version '{1}'",
                        questionnaireId, version));
        }

        private string GetFolderPathOfDataByQuestionnaire(Guid questionnaireId, long version)
        {
            return fileSystemAccessor.CombinePath(path, string.Format("exported_data_{0}_{1}", questionnaireId, version));
        }

        private string CreateValidFileName(string name, HashSet<string> createdFileNames, int i =0)
        {
            string fileNameWithoutInvalidFileNameChars = fileSystemAccessor.MakeValidFileName(name);
            string fileNameWithNumber = string.Concat(fileNameWithoutInvalidFileNameChars,
                                                         i == 0 ? (object)string.Empty : i);

            return !createdFileNames.Contains(fileNameWithNumber)
                       ? fileNameWithNumber
                       : this.CreateValidFileName(name, createdFileNames, i + 1);
        }
    }
}
