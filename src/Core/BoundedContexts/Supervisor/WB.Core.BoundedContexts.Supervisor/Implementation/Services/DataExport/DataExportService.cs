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
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Supervisor.Implementation.Services.DataExport
{
    internal class DataExportService : IDataExportService
    {
        private const string FolderName = "ExportedData";
        private readonly string path;
        private readonly FileType fileType = FileType.Csv;

        public DataExportService(string folderPath)
        {
            this.path = Path.Combine(folderPath, FolderName);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        public string GetFilePathToExportedCompressedData(Guid questionnaireId, long version, string type)
        {
            var dataDirectoryPath = GetFolderPath(questionnaireId, version);

            if (!Directory.Exists(dataDirectoryPath))
                return string.Empty;
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

        public Dictionary<Guid, string> GetLevelIdToDataFilePathMap(InterviewDataExportView interviewDataExportView)
        {
            var dataFolderForTemplatePath = GetFolderPath(interviewDataExportView.TemplateId, interviewDataExportView.TemplateVersion);
            var result = new Dictionary<Guid, string>();
            var createdFileNames = new HashSet<string>();
            foreach (var interviewDataExportLevelView in interviewDataExportView.Levels)
            {
                string fileName = GetName(interviewDataExportLevelView.LevelName, createdFileNames, 0);
                var dataFileNameWithExtension = string.Format("{0}.{1}", fileName, this.GetFileExtension(fileType));
                result[interviewDataExportLevelView.LevelId]=Path.Combine(dataFolderForTemplatePath, dataFileNameWithExtension);
                createdFileNames.Add(fileName);
            }
            return result;
        }

        public void CreateDataFolderForTemplate(QuestionnaireExportStructure questionnaireExportStructure)
        {
            var dataFolderForTemplatePath = GetFolderPath(questionnaireExportStructure.QuestionnaireId, questionnaireExportStructure.Version);
            if (Directory.Exists(dataFolderForTemplatePath))
                Directory.Delete(dataFolderForTemplatePath, true);
         
            Directory.CreateDirectory(dataFolderForTemplatePath);

            var createdFileNames = new HashSet<string>();
            var interviewExporter = new IterviewExporter();

            foreach (var headerStructureForLevel in questionnaireExportStructure.HeaderToLevelMap)
            {
                string fileName = GetName(headerStructureForLevel.Value.LevelName, createdFileNames, 0);
                createdFileNames.Add(fileName);
                var dataFileNameWithExtension = string.Format("{0}.{1}", fileName, this.GetFileExtension(fileType));
                var stataContent = new StataEnvironmentContentGenerator(headerStructureForLevel.Value, fileName,
                    fileType,
                    dataFileNameWithExtension);

                File.WriteAllBytes(Path.Combine(dataFolderForTemplatePath, dataFileNameWithExtension),
                    interviewExporter.CreateHeader(headerStructureForLevel.Value));
                File.WriteAllBytes(Path.Combine(dataFolderForTemplatePath, stataContent.NameOfAdditionalFile),
                    stataContent.ContentOfAdditionalFile);
            }
        }

        private string GetFolderPath(Guid questionnaireId, long version)
        {
            return Path.Combine(path, GetFolderName(questionnaireId, version));
        }

        private string GetFolderName(Guid questionnaireId, long version)
        {
            return string.Format("exported_data_{0}_{1}", questionnaireId, version);
        }

        private FileType GetFileTypeOrThrow(string type)
        {
            if (type != InterviewExportConstants.CSVFORMAT && type != InterviewExportConstants.TABFORMAT)
                throw new InvalidOperationException("file type doesn't support");
            return type == InterviewExportConstants.CSVFORMAT ? FileType.Csv : FileType.Tab;
        }

        protected string GetName(string name, HashSet<string> createdFileNames, int i)
        {
            string fileNameWithoutInvalidFileNameChars = Path.GetInvalidFileNameChars()
                                                             .Aggregate(name, (current, c) => current.Replace(c, '_'));
            string fileNameWithNumber = string.Concat(RemoveNonAscii(fileNameWithoutInvalidFileNameChars),
                                                         i == 0 ? (object)string.Empty : i);

            var validFileName = MakeValidFileName(fileNameWithNumber);

            return !createdFileNames.Contains(validFileName)
                       ? validFileName
                       : this.GetName(name, createdFileNames, i + 1);
        }

        protected string GetFileExtension(FileType type)
        {
            return type == FileType.Csv ? InterviewExportConstants.CSVFORMAT : InterviewExportConstants.TABFORMAT;
        }

        protected string RemoveNonAscii(string s)
        {
            return Regex.Replace(s, @"[^\u0000-\u007F]", string.Empty);
        }

        public string MakeValidFileName(string name)
        {
            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidReStr = String.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);
            return Regex.Replace(name, invalidReStr, "_");
        }
    }
}
