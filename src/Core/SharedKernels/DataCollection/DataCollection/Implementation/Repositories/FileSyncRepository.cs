using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Repositories
{
    internal class FileSyncRepository : IFileSyncRepository
    {
        private readonly IPlainFileRepository plainFileRepository;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private const string SyncDirectoryName = "SYNC";
        private readonly string basePath;

        public FileSyncRepository(IPlainFileRepository plainFileRepository, IFileSystemAccessor fileSystemAccessor, string rootDirectoryPath)
        {
            this.plainFileRepository = plainFileRepository;
            this.fileSystemAccessor = fileSystemAccessor;

            this.basePath = this.fileSystemAccessor.CombinePath(rootDirectoryPath, SyncDirectoryName);

            if (!this.fileSystemAccessor.IsDirectoryExists(this.basePath))
                this.fileSystemAccessor.CreateDirectory(this.basePath);
        }

        public void MoveInterviewsBinaryDataToSyncFolder(Guid interviewId)
        {
            var interviewDirectoryPath = this.GetPathToInterviewDirectory(interviewId);

            if (!fileSystemAccessor.IsDirectoryExists(interviewDirectoryPath))
                fileSystemAccessor.CreateDirectory(interviewDirectoryPath);

            var files = plainFileRepository.GetBinaryFilesForInterview(interviewId);
            foreach (var file in files)
            {
                fileSystemAccessor.WriteAllBytes(GetPathToFile(interviewId, file.FileName), file.Data);
            }
        }

        public IList<InterviewBinaryData> GetBinaryFilesFromSyncFolder()
        {
            var result = new List<InterviewBinaryData>();

            foreach (var syncInterviewDirectory in fileSystemAccessor.GetDirectoriesInDirectory(basePath))
            {
                var directoryName = fileSystemAccessor.GetFileName(syncInterviewDirectory);
                Guid interviewId;
                if (!Guid.TryParse(directoryName, out interviewId))
                    continue;
                result.AddRange(
                    fileSystemAccessor.GetFilesInDirectory(syncInterviewDirectory)
                        .Select(
                            fileName =>
                                new InterviewBinaryData(interviewId, fileSystemAccessor.GetFileName(fileName),
                                    () => fileSystemAccessor.ReadAllBytes(fileName))));
            }

            return result;
        }

        public void RemoveBinaryDataFromSyncFolder(Guid interviewId, string fileName)
        {
            var interviewSyncDirectory = this.GetPathToInterviewDirectory(interviewId);

            var pathToFile = fileSystemAccessor.CombinePath(interviewSyncDirectory, fileName);
            if (fileSystemAccessor.IsFileExists(pathToFile))
                fileSystemAccessor.DeleteFile(pathToFile);
        }

        private string GetPathToInterviewDirectory(Guid interviewId)
        {
            return fileSystemAccessor.CombinePath(basePath, interviewId.FormatGuid());
        }

        private string GetPathToFile(Guid interviewId, string fileName)
        {
            return fileSystemAccessor.CombinePath(GetPathToInterviewDirectory(interviewId), fileName);
        }
    }
}
