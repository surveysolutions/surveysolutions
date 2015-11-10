using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.DataExport.QueuedProcess;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers
{
    public class BinaryFormatDataExportProcessHandler : IExportProcessHandler<AllDataQueuedProcess>
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IPlainInterviewFileStorage plainFileRepository;
        private readonly IFilebasedExportedDataAccessor filebasedExportedDataAccessor;

        private readonly IArchiveUtils archiveUtils;
        private readonly ITransactionManager transactionManager;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaries;

        private const string temporaryTabularExportFolder = "TemporaryBinaryExport";
        private readonly string pathToExportedData;

        public BinaryFormatDataExportProcessHandler(IFileSystemAccessor fileSystemAccessor, IPlainInterviewFileStorage plainFileRepository, IFilebasedExportedDataAccessor filebasedExportedDataAccessor,
            InterviewDataExportSettings interviewDataExportSettings, ITransactionManager transactionManager, IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaries, IArchiveUtils archiveUtils)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.plainFileRepository = plainFileRepository;
            this.filebasedExportedDataAccessor = filebasedExportedDataAccessor;
            this.transactionManager = transactionManager;
            this.interviewSummaries = interviewSummaries;
            this.archiveUtils = archiveUtils;

            this.pathToExportedData = fileSystemAccessor.CombinePath(interviewDataExportSettings.DirectoryPath, temporaryTabularExportFolder);

            if (!fileSystemAccessor.IsDirectoryExists(this.pathToExportedData))
                fileSystemAccessor.CreateDirectory(this.pathToExportedData);
        }

        public void ExportData(AllDataQueuedProcess process)
        {
            List<Guid> interviewIdsToExport =
                this.transactionManager.ExecuteInQueryTransaction(() =>
                    this.interviewSummaries.Query(_ =>
                        _.Where(x => x.QuestionnaireId == process.QuestionnaireIdentity.QuestionnaireId &&
                                     x.QuestionnaireVersion == process.QuestionnaireIdentity.Version)
                            .OrderBy(x => x.InterviewId)
                            .Select(x => x.InterviewId).ToList()));

            string folderForDataExport = GetFolderPathOfDataByQuestionnaire(process.QuestionnaireIdentity);

            this.ClearFolder(folderForDataExport);

            foreach (var interviewId in interviewIdsToExport)
            {
                var interviewBinaryFiles = plainFileRepository.GetBinaryFilesForInterview(interviewId);
                var filesFolderForInterview = this.fileSystemAccessor.CombinePath(folderForDataExport,
                    interviewId.FormatGuid());

                if(interviewBinaryFiles.Count>0)
                    this.fileSystemAccessor.CreateDirectory(filesFolderForInterview);

                foreach (var interviewBinaryFile in interviewBinaryFiles)
                {
                    this.fileSystemAccessor.WriteAllBytes(
                     this.fileSystemAccessor.CombinePath(filesFolderForInterview,
                         interviewBinaryFile.FileName), interviewBinaryFile.GetData());
                }
            }

            var archiveFilePath = this.filebasedExportedDataAccessor.GetArchiveFilePathForExportedData(process.QuestionnaireIdentity, DataExportFormat.Binary);
            RecreateExportArchive(folderForDataExport, archiveFilePath);
        }

        private string GetFolderPathOfDataByQuestionnaire(QuestionnaireIdentity questionnaireIdentity)
        {
            return this.fileSystemAccessor.CombinePath(this.pathToExportedData,
                $"{questionnaireIdentity.QuestionnaireId}_{questionnaireIdentity.Version}");
        }

        private void ClearFolder(string folderName)
        {
            if (this.fileSystemAccessor.IsDirectoryExists(folderName))
                this.fileSystemAccessor.DeleteDirectory(folderName);

            this.fileSystemAccessor.CreateDirectory(folderName);
        }
        private void RecreateExportArchive(string folderForDataExport, string archiveFilePath)
        {
            if (this.fileSystemAccessor.IsFileExists(archiveFilePath))
            {
                this.fileSystemAccessor.DeleteFile(archiveFilePath);
            }

            this.archiveUtils.ZipDirectory(folderForDataExport, archiveFilePath);
        }
    }
}