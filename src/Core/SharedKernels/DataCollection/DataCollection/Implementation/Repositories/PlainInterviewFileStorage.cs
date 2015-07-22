using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Repositories
{
    internal class PlainInterviewFileStorage : IPlainInterviewFileStorage
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly string basePath;

        public PlainInterviewFileStorage(IFileSystemAccessor fileSystemAccessor, string rootDirectoryPath, string dataDirectoryName)
        {
            this.fileSystemAccessor = fileSystemAccessor;

            this.basePath = this.fileSystemAccessor.CombinePath(rootDirectoryPath, dataDirectoryName);

            if (!this.fileSystemAccessor.IsDirectoryExists(this.basePath))
                this.fileSystemAccessor.CreateDirectory(this.basePath);
        }

        public byte[] GetInterviewBinaryData(Guid interviewId, string fileName)
        {
            var filePath = this.GetPathToFile(interviewId, fileName);
            if (!fileSystemAccessor.IsFileExists(filePath))
                return null;
            return fileSystemAccessor.ReadAllBytes(filePath);
        }

        public List<InterviewBinaryDataDescriptor> GetBinaryFilesForInterview(Guid interviewId)
        {
            var directoryPath = this.GetPathToInterviewDirectory(interviewId);
            
            if (!fileSystemAccessor.IsDirectoryExists(directoryPath))
                return new List<InterviewBinaryDataDescriptor>();

            return fileSystemAccessor.GetFilesInDirectory(directoryPath)
                .Select(
                    fileName =>
                        new InterviewBinaryDataDescriptor(interviewId, fileSystemAccessor.GetFileName(fileName),
                            () => fileSystemAccessor.ReadAllBytes(fileName))).ToList();
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

        private string GetPathToFile(Guid interviewId, string fileName)
        {
            return fileSystemAccessor.CombinePath(GetPathToInterviewDirectory(interviewId), fileName);
        }

        private string GetPathToInterviewDirectory(Guid interviewId, string baseDirectory=null)
        {
            return fileSystemAccessor.CombinePath(baseDirectory ?? basePath, interviewId.FormatGuid());
        }
    }
}
