using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using Ionic.Zip;
using Ionic.Zlib;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading
{
    internal class CsvPreloadingTemplateService : IPreloadingTemplateService
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly string delimiter = ",";
        private const string FolderName = "PreLoadingTemplates";
        private readonly string path;
        private readonly IVersionedReadSideRepositoryReader<QuestionnaireExportStructure> questionnaireDocumentVersionedStorage;


        public CsvPreloadingTemplateService(IFileSystemAccessor fileSystemAccessor,
            IVersionedReadSideRepositoryReader<QuestionnaireExportStructure> questionnaireDocumentVersionedStorage, string folderPath)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.questionnaireDocumentVersionedStorage = questionnaireDocumentVersionedStorage;
            this.path = fileSystemAccessor.CombinePath(folderPath, FolderName);
            if (!fileSystemAccessor.IsDirectoryExists(this.path))
                fileSystemAccessor.CreateDirectory(this.path);
        }

        public string GetFilePathToPreloadingTemplate(Guid questionnaireId, long version)
        {
            var questionnaire = this.questionnaireDocumentVersionedStorage.GetById(questionnaireId, version);
            if (questionnaire == null)
                return null;

         
            var firstLevelHeader = questionnaire.HeaderToLevelMap[questionnaireId];

            var dataDirectoryPath = this.GetFolderPathOfDataByQuestionnaire(firstLevelHeader.LevelName);
            if (fileSystemAccessor.IsDirectoryExists(dataDirectoryPath))
                fileSystemAccessor.DeleteDirectory(dataDirectoryPath);
            
            fileSystemAccessor.CreateDirectory(dataDirectoryPath);
            var archiveFilePath = this.fileSystemAccessor.CombinePath(this.path, string.Format("{0}.zip", this.fileSystemAccessor.GetFileName(dataDirectoryPath)));


            var interviewTemplateFilePath = this.fileSystemAccessor.CombinePath(dataDirectoryPath,
                GetLevelFileName(firstLevelHeader.LevelName));

            using (var fileStream = fileSystemAccessor.OpenOrCreateFile(interviewTemplateFilePath))
            using (var streamWriter = new StreamWriter(fileStream, Encoding.UTF8))
            using (var writer = new CsvWriter(streamWriter))
            {
                writer.Configuration.Delimiter = this.delimiter;
                writer.WriteField(firstLevelHeader.LevelIdColumnName);

                foreach (ExportedHeaderItem question in firstLevelHeader.HeaderItems.Values)
                {
                    foreach (var columnName in question.ColumnNames)
                    {
                        writer.WriteField(columnName);
                    }
                }
                writer.WriteField("ParentId");
                writer.NextRecord();
                streamWriter.Flush();
            }

            using (var zip = new ZipFile(this.fileSystemAccessor.GetFileName(archiveFilePath)))
            {
                zip.CompressionLevel = CompressionLevel.BestCompression;
                zip.AddDirectory(dataDirectoryPath);
                zip.Save(archiveFilePath);
            }

            return archiveFilePath;
        }

        private string GetFolderPathOfDataByQuestionnaire(string title)
        {
            return this.fileSystemAccessor.CombinePath(this.path, string.Format("preloading_template_{0}", this.fileSystemAccessor.MakeValidFileName(title)));
        }

        public string GetLevelFileName(string levelName)
        {
            return string.Format("{0}.csv", this.fileSystemAccessor.MakeValidFileName(levelName));
        }
    }
}
