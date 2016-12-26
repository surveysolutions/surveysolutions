using System;
using System.Collections.Generic;
using System.Globalization;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.Export;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Infrastructure.Security;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Accessors
{
    internal class TabularParaDataAccessor : IParaDataAccessor
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IZipArchiveProtectionService archiveUtils;
        private readonly ICsvWriter csvWriter;
        private readonly IExportSettings exportSettings;
        private readonly IPlainTransactionManagerProvider plainTransactionManagerProvider;
        private readonly IExportFileNameService exportFileNameService;
        private readonly string pathToHistoryFiles;

        private IPlainTransactionManager PlainTransactionManager => this.plainTransactionManagerProvider.GetPlainTransactionManager();


        public TabularParaDataAccessor(
            IFileSystemAccessor fileSystemAccessor,
            InterviewDataExportSettings interviewDataExportSettings,
            IZipArchiveProtectionService archiveUtils, 
            ICsvWriter csvWriter,
            IExportSettings exportSettings,
            IPlainTransactionManagerProvider plainTransactionManagerProvider,
            IExportFileNameService exportFileNameService)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.archiveUtils = archiveUtils;
            this.csvWriter = csvWriter;
            this.exportSettings = exportSettings;
            this.plainTransactionManagerProvider = plainTransactionManagerProvider;
            this.exportFileNameService = exportFileNameService;

            this.pathToHistoryFiles = fileSystemAccessor.CombinePath(interviewDataExportSettings.DirectoryPath,
                interviewDataExportSettings.ExportedDataFolderName);

            if (!fileSystemAccessor.IsDirectoryExists(this.pathToHistoryFiles))
                fileSystemAccessor.CreateDirectory(this.pathToHistoryFiles);
        }


        public void ClearParaDataFolder()
        {
            if (this.fileSystemAccessor.IsDirectoryExists(this.pathToHistoryFiles))
            {
                Array.ForEach(this.fileSystemAccessor.GetDirectoriesInDirectory(this.pathToHistoryFiles), (s) => this.fileSystemAccessor.DeleteDirectory(s));
            }
        }

        public void ClearParaDataFile()
        {
            if (this.fileSystemAccessor.IsDirectoryExists(this.pathToHistoryFiles))
            {
                Array.ForEach(this.fileSystemAccessor.GetFilesInDirectory(this.pathToHistoryFiles, "*.zip"), (s) => this.fileSystemAccessor.DeleteFile(s));
            }
        }

        public void ArchiveParaDataFolder()
        {
            var createdFolders = this.fileSystemAccessor.GetDirectoriesInDirectory(this.pathToHistoryFiles);
            foreach (var createdFolder in createdFolders)
            {
                var archiveFilePath = createdFolder + ".zip";

                if (this.fileSystemAccessor.IsFileExists(archiveFilePath))
                    this.fileSystemAccessor.DeleteFile(archiveFilePath);

                var filesToArchive = new List<string>();

                filesToArchive.AddRange(this.fileSystemAccessor.GetFilesInDirectory(createdFolder));

                var password = this.GetPasswordFromSettings();
                this.archiveUtils.ZipFiles(filesToArchive, archiveFilePath, password);
            }
        }

        private string GetPasswordFromSettings()
        {
            return this.PlainTransactionManager.ExecuteInPlainTransaction(() =>
                this.exportSettings.EncryptionEnforced()
                    ? this.exportSettings.GetPassword()
                    : null);
        }


        public void StoreInterviewParadata(InterviewHistoryView view)
        {
            var questionnairePath = GetPathToFolderWithParaDataByQuestionnaire(view.QuestionnaireId, view.QuestionnaireVersion);

            if (!this.fileSystemAccessor.IsDirectoryExists(questionnairePath))
                this.fileSystemAccessor.CreateDirectory(questionnairePath);

            using (var fileStream =
                    this.fileSystemAccessor.OpenOrCreateFile(this.GetPathToInterviewHistoryFile(view.InterviewId, view.QuestionnaireId, view.QuestionnaireVersion), true))

            using (var writer = this.csvWriter.OpenCsvWriter(fileStream, ExportFileSettings.SeparatorOfExportedDataFile.ToString()))
            {
                foreach (var interviewHistoricalRecordView in view.Records)
                {
                    writer.WriteField(interviewHistoricalRecordView.Action);
                    writer.WriteField(interviewHistoricalRecordView.OriginatorName);
                    writer.WriteField(interviewHistoricalRecordView.OriginatorRole);
                    if (interviewHistoricalRecordView.Timestamp.HasValue)
                    {
                        writer.WriteField(interviewHistoricalRecordView.Timestamp.Value.ToString("d",
                            CultureInfo.InvariantCulture));
                        writer.WriteField(interviewHistoricalRecordView.Timestamp.Value.ToString("T",
                            CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        writer.WriteField(string.Empty);
                        writer.WriteField(string.Empty);
                    }
                    foreach (var value in interviewHistoricalRecordView.Parameters.Values)
                    {
                        writer.WriteField(value ?? string.Empty);
                    }
                    writer.NextRecord();
                }
            }
        }

        public  string GetPathToParaDataArchiveByQuestionnaire(Guid questionnaireId, long version)
        {
            return GetPathToFolderWithParaDataByQuestionnaire(questionnaireId, version) + ".zip";
        }

        private string GetPathToInterviewHistoryFile(Guid interviewId, Guid questionnaireId, long version)
        {
            return this.fileSystemAccessor.CombinePath(GetPathToFolderWithParaDataByQuestionnaire(questionnaireId, version), $"{interviewId.FormatGuid()}.tab");
        }

        private string GetPathToFolderWithParaDataByQuestionnaire(Guid questionnaireId, long version)
        {
            return this.exportFileNameService.GetFolderNameForParaDataByQuestionnaire(new QuestionnaireIdentity(questionnaireId, version), this.pathToHistoryFiles);
        }
    }
}
