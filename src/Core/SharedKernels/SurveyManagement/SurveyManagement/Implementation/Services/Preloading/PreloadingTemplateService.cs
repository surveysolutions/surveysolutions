﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using Ionic.Zip;
using Ionic.Zlib;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading
{
    internal class PreloadingTemplateService : IPreloadingTemplateService
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IDataFileExportService dataFileExportService;
        private readonly IArchiveUtils archiveUtils;
        private readonly IRosterDataService rosterDataService;
        private const string FolderName = "PreLoadingTemplates";
        private readonly string path;
        private readonly IVersionedReadSideRepositoryReader<QuestionnaireExportStructure> questionnaireDocumentVersionedStorage;


        public PreloadingTemplateService(IFileSystemAccessor fileSystemAccessor,
            IVersionedReadSideRepositoryReader<QuestionnaireExportStructure> questionnaireDocumentVersionedStorage, string folderPath,
            IDataFileExportService dataFileExportService, IArchiveUtils archiveUtils, IRosterDataService rosterDataService)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.questionnaireDocumentVersionedStorage = questionnaireDocumentVersionedStorage;
            this.dataFileExportService = dataFileExportService;
            this.archiveUtils = archiveUtils;
            this.rosterDataService = rosterDataService;
            this.path = fileSystemAccessor.CombinePath(folderPath, FolderName);
            if (!fileSystemAccessor.IsDirectoryExists(this.path))
                fileSystemAccessor.CreateDirectory(this.path);
        }

        public string GetFilePathToPreloadingTemplate(Guid questionnaireId, long version)
        {
            var questionnaire = this.questionnaireDocumentVersionedStorage.GetById(questionnaireId, version);
            if (questionnaire == null)
                return null;

            var dataDirectoryPath = this.GetFolderPathOfDataByQuestionnaire(questionnaireId, version);
            
            if (!fileSystemAccessor.IsDirectoryExists(dataDirectoryPath))
            {
                fileSystemAccessor.CreateDirectory(dataDirectoryPath);
            }

            var archiveFilePath = this.fileSystemAccessor.CombinePath(this.path, string.Format("{0}.zip", this.fileSystemAccessor.GetFileName(dataDirectoryPath)));

            if (fileSystemAccessor.IsFileExists(archiveFilePath))
                return archiveFilePath;

            var cleanedFileNamesForLevels =
              rosterDataService.CreateCleanedFileNamesForLevels(
                  questionnaire.HeaderToLevelMap.Values.ToDictionary(h => h.LevelId, h => h.LevelName));
            foreach (var header in questionnaire.HeaderToLevelMap.Values)
            {
                var interviewTemplateFilePath = this.fileSystemAccessor.CombinePath(dataDirectoryPath,
                    dataFileExportService.GetInterviewExportedDataFileName(cleanedFileNamesForLevels[header.LevelId]));

                dataFileExportService.CreateHeader(header, interviewTemplateFilePath);
            }
            archiveUtils.ZipDirectory(dataDirectoryPath, archiveFilePath);

            return archiveFilePath;
        }

        private string GetFolderPathOfDataByQuestionnaire(Guid questionnaireId, long version)
        {
            return this.fileSystemAccessor.CombinePath(this.path,
                this.fileSystemAccessor.MakeValidFileName(string.Format("template_{0}_v{1}", questionnaireId.FormatGuid(), version)));
        }
    }
}
