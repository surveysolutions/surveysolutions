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

        private string GetPathToInterviewDirectory(Guid interviewId)
        {
            return fileSystemAccessor.CombinePath(basePath, interviewId.FormatGuid());
        }
    }
}
