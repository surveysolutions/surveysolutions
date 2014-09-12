using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Repositories
{
    internal class PlainFileRepository : IPlainFileRepository
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly string basePath;
        private const string SyncDirectoryName = "SYNC";

        public PlainFileRepository(IFileSystemAccessor fileSystemAccessor, string basePath, string directoryName)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.basePath = basePath;
        }

        public byte[] GetInterviewBinaryData(Guid interviewId, string fileName)
        {
            var filePath = this.GetPathToFile(interviewId, fileName);
            if (!fileSystemAccessor.IsFileExists(filePath))
                return null;
            return fileSystemAccessor.ReadAllBytes(filePath);
        }

        public void StoreInterviewBinaryData(Guid interviewId, string fileName, byte[] data)
        {
            var directoryPath = this.GetPathToInterviewDirectory(interviewId);
            
            if (!fileSystemAccessor.IsDirectoryExists(directoryPath))
                fileSystemAccessor.CreateDirectory(directoryPath);

            fileSystemAccessor.WriteAllBytes(this.GetPathToFile(interviewId, fileName), data);
        }

        public void RemoveInterviewBinaryData(Guid interviewId, string fileName)
        {
            var filePath = this.GetPathToFile(interviewId, fileName);
            if (!fileSystemAccessor.IsFileExists(filePath))
                return;

            fileSystemAccessor.DeleteFile(filePath);
        }

        public void RemoveAllBinaryDataForInterview(Guid interviewId)
        {
            var directoryPath = this.GetPathToInterviewDirectory(interviewId);
            if (!fileSystemAccessor.IsDirectoryExists(directoryPath))
                return;

            fileSystemAccessor.DeleteDirectory(directoryPath);
        }

        public void MoveInterviewsBinaryDataToSyncFolder(Guid interviewId)
        {
            var pathToInterviewDirectory = this.GetPathToInterviewDirectory(interviewId);
             if (!fileSystemAccessor.IsDirectoryExists(pathToInterviewDirectory))
               return;

            var syncDirectoryPath = this.GetPathToSyncFolder();

            if (!fileSystemAccessor.IsDirectoryExists(syncDirectoryPath))
                fileSystemAccessor.CreateDirectory(syncDirectoryPath);

            fileSystemAccessor.CopyFileOrDirectory(pathToInterviewDirectory, syncDirectoryPath);
        }

        public IList<InterviewBinaryData> GetBinaryFilesFromSyncFolder()
        {
            var syncDirectoryPath = this.GetPathToSyncFolder();

            if (!fileSystemAccessor.IsDirectoryExists(syncDirectoryPath))
                return new InterviewBinaryData[0];

            var result = new List<InterviewBinaryData>();

            foreach (var syncInterviewDirectory in fileSystemAccessor.GetDirectoriesInDirectory(syncDirectoryPath))
            {
                var directoryName = fileSystemAccessor.GetFileName(syncInterviewDirectory);
                Guid interviewId;
                if (!Guid.TryParse(directoryName, out interviewId))
                    continue;
                result.AddRange(
                    fileSystemAccessor.GetFilesInDirectory(syncDirectoryPath)
                        .Select(
                            fileName =>
                                new InterviewBinaryData(interviewId, fileSystemAccessor.GetFileName(fileName),
                                    () => fileSystemAccessor.ReadAllBytes(fileName))));
            }

            return result;
        }

        public void RemoveBinaryDataFromSyncFolder(Guid interviewId, string fileName)
        {
            var syncDirectoryPath = this.GetPathToSyncFolder();

            if (!fileSystemAccessor.IsDirectoryExists(syncDirectoryPath))
                return;

            var interviewSyncDirectory = this.GetPathToInterviewDirectory(interviewId, syncDirectoryPath);

            var pathToFile = fileSystemAccessor.CombinePath(interviewSyncDirectory, fileName);
            if (fileSystemAccessor.IsFileExists(pathToFile))
                fileSystemAccessor.DeleteFile(pathToFile);
        }

        public string[] GetAllIdsOfBinaryDataByInterview(Guid interviewId)
        {
            var directoryPath = this.GetPathToInterviewDirectory(interviewId);
            if (!fileSystemAccessor.IsDirectoryExists(directoryPath))
                return new string[0];

            return fileSystemAccessor.GetFilesInDirectory(directoryPath);
        }

        private string GetPathToFile(Guid interviewId, string fileName)
        {
            return fileSystemAccessor.CombinePath(GetPathToInterviewDirectory(interviewId), fileName);
        }

        private string GetPathToInterviewDirectory(Guid interviewId, string baseDirectory=null)
        {
            return fileSystemAccessor.CombinePath(baseDirectory ?? basePath, interviewId.FormatGuid());
        }

        private string GetPathToSyncFolder()
        {
            return fileSystemAccessor.CombinePath(basePath, SyncDirectoryName);
        }
    }
}
