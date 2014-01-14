using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Ionic.Zip;
using Ionic.Zlib;
using Main.Core.View;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Supervisor.Implementation.Services.DataExport
{
    internal class DataExportService : IDataExportService
    {
        private const string FolderName = "ExportedData";
        private readonly string path;
        private readonly CsvInterviewExporter csvInterviewExporter = new CsvInterviewExporter();

        public DataExportService(IReadSideRepositoryCleanerRegistry cleanerRegistry, string folderPath)
        {
            cleanerRegistry.Register(this);
            this.path = Path.Combine(folderPath, FolderName);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        public string GetFilePathToExportedCompressedData(Guid questionnaireId, long version)
        {
            var dataDirectoryPath = GetFolderPathOfDataByQuestionnaire(questionnaireId, version);

            ThrowArgumentExceptionIfDataFolderMissing(questionnaireId, version, dataDirectoryPath);

            var archiveFilePath = Path.Combine(path, string.Format("{0}.zip", Path.GetFileName(dataDirectoryPath)));

            if (File.Exists(archiveFilePath))
                File.Delete(archiveFilePath);

            using (var zip = new ZipFile(Path.GetFileName(archiveFilePath)))
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
            
            if (Directory.Exists(dataFolderForTemplatePath))
                Directory.Delete(dataFolderForTemplatePath, true);
            Directory.CreateDirectory(dataFolderForTemplatePath);

            var createdFileNames = new HashSet<string>();

            foreach (var headerStructureForLevel in questionnaireExportStructure.HeaderToLevelMap)
            {
                string fileName = CreateValidFileName(headerStructureForLevel.Value.LevelName, createdFileNames, 0);
                createdFileNames.Add(fileName);
                var dataFileNameWithExtension = string.Format("{0}.csv", fileName);
                var stataContent = new StataEnvironmentContentGenerator(headerStructureForLevel.Value, fileName,
                    dataFileNameWithExtension);

                File.WriteAllBytes(Path.Combine(dataFolderForTemplatePath, dataFileNameWithExtension),
                    csvInterviewExporter.CreateHeader(headerStructureForLevel.Value));
                File.WriteAllBytes(Path.Combine(dataFolderForTemplatePath, stataContent.NameOfAdditionalFile),
                    stataContent.ContentOfAdditionalFile);
            }
        }

        public void DeleteExportedData(Guid questionnaireId, long version)
        {
            var dataFolderForTemplatePath = GetFolderPathOfDataByQuestionnaire(questionnaireId, version);
            if (Directory.Exists(dataFolderForTemplatePath))
                Directory.Delete(dataFolderForTemplatePath, true);
        }

        public void AddExportedDataByInterview(InterviewDataExportView interviewDataExportView)
        {
            var dataFolderForTemplatePath = GetFolderPathOfDataByQuestionnaire(interviewDataExportView.TemplateId, interviewDataExportView.TemplateVersion);

            ThrowArgumentExceptionIfDataFolderMissing(interviewDataExportView.TemplateId, interviewDataExportView.TemplateVersion, dataFolderForTemplatePath);

            var createdFileNames = new HashSet<string>();
            foreach (var interviewDataExportLevelView in interviewDataExportView.Levels)
            {
                string fileName = CreateValidFileName(interviewDataExportLevelView.LevelName, createdFileNames, 0);
                var dataFileNameWithExtension = string.Format("{0}.csv", fileName);
                var pathToLevelDataFile = Path.Combine(dataFolderForTemplatePath, dataFileNameWithExtension);
                csvInterviewExporter.AddRecord(pathToLevelDataFile,interviewDataExportLevelView);
                createdFileNames.Add(fileName);
            }
        }

        private void ThrowArgumentExceptionIfDataFolderMissing(Guid questionnaireId, long version, string dataDirectoryPath)
        {
            if (!Directory.Exists(dataDirectoryPath))
                throw new ArgumentException(
                    string.Format("data files are absent for questionnaire with id '{0}' and version '{1}'",
                        questionnaireId, version));
        }

        private string GetFolderPathOfDataByQuestionnaire(Guid questionnaireId, long version)
        {
            return Path.Combine(path, string.Format("exported_data_{0}_{1}", questionnaireId, version));
        }

        private string CreateValidFileName(string name, HashSet<string> createdFileNames, int i)
        {
            string fileNameWithoutInvalidFileNameChars = Path.GetInvalidFileNameChars()
                                                             .Aggregate(name, (current, c) => current.Replace(c, '_'));
            string fileNameWithNumber = string.Concat(RemoveNonAscii(fileNameWithoutInvalidFileNameChars),
                                                         i == 0 ? (object)string.Empty : i);

            var validFileName = MakeValidFileName(fileNameWithNumber);

            return !createdFileNames.Contains(validFileName)
                       ? validFileName
                       : this.CreateValidFileName(name, createdFileNames, i + 1);
        }

        private string RemoveNonAscii(string s)
        {
            return Regex.Replace(s, @"[^\u0000-\u007F]", string.Empty);
        }

        private string MakeValidFileName(string name)
        {
            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidReStr = String.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);
            return Regex.Replace(name, invalidReStr, "_");
        }

        public void Clear()
        {
            Array.ForEach(Directory.GetFiles(path), File.Delete);
        }
    }
}
