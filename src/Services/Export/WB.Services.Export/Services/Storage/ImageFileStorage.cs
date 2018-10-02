using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Utils;

namespace WB.Services.Export.Services.Storage
{
    public class ImageFileStorage : IImageFileStorage
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly string basePath;
        private const string DataDirectoryName = "InterviewData";

        public ImageFileStorage(IFileSystemAccessor fileSystemAccessor, string rootDirectoryPath)
        {
            this.fileSystemAccessor = fileSystemAccessor;

            this.basePath = this.fileSystemAccessor.CombinePath(rootDirectoryPath, DataDirectoryName);

            if (!this.fileSystemAccessor.IsDirectoryExists(this.basePath))
                this.fileSystemAccessor.CreateDirectory(this.basePath);
        }

        public Task<byte[]> GetInterviewBinaryData(Guid interviewId, string fileName)
        {
            var filePath = this.GetPathToFile(interviewId, fileName);
            if (!fileSystemAccessor.IsFileExists(filePath))
                return null;
            return Task.FromResult(fileSystemAccessor.ReadAllBytes(filePath));
        }

        public Task<List<InterviewBinaryDataDescriptor>> GetBinaryFilesForInterview(Guid interviewId)
        {
            var directoryPath = this.GetPathToInterviewDirectory(interviewId);

            if (!fileSystemAccessor.IsDirectoryExists(directoryPath))
                return Task.FromResult(new List<InterviewBinaryDataDescriptor>());

            var result = fileSystemAccessor.GetFilesInDirectory(directoryPath)
                .Select(
                    fileName =>
                        new InterviewBinaryDataDescriptor(
                            interviewId,
                            fileSystemAccessor.GetFileName(fileName),
                            null,
                            () => Task.FromResult(fileSystemAccessor.ReadAllBytes(fileName)))).ToList();

            return Task.FromResult(result);
        }

        public Task StoreInterviewBinaryData(Guid interviewId, string fileName, byte[] data, string contentType)
        {
            var directoryPath = this.GetPathToInterviewDirectory(interviewId);

            if (!fileSystemAccessor.IsDirectoryExists(directoryPath))
                fileSystemAccessor.CreateDirectory(directoryPath);

            var filePath = this.GetPathToFile(interviewId, fileName);

            if (fileSystemAccessor.IsFileExists(filePath))
                fileSystemAccessor.DeleteFile(filePath);

            fileSystemAccessor.WriteAllBytes(filePath, data);

            return Task.CompletedTask;
        }

        public Task RemoveInterviewBinaryData(Guid interviewId, string fileName)
        {
            var filePath = this.GetPathToFile(interviewId, fileName);
            if (!fileSystemAccessor.IsFileExists(filePath))
                return Task.CompletedTask;

            fileSystemAccessor.DeleteFile(filePath);
            return Task.CompletedTask;
        }

        public Task RemoveAllBinaryDataForInterview(Guid interviewId)
        {
            var directoryPath = this.GetPathToInterviewDirectory(interviewId);
            if (!fileSystemAccessor.IsDirectoryExists(directoryPath))
                return Task.CompletedTask;

            fileSystemAccessor.DeleteDirectory(directoryPath);
            return Task.CompletedTask;
        }

        private string GetPathToFile(Guid interviewId, string fileName)
        {
            return fileSystemAccessor.CombinePath(GetPathToInterviewDirectory(interviewId), fileName);
        }

        private string GetPathToInterviewDirectory(Guid interviewId, string baseDirectory = null)
        {
            return fileSystemAccessor.CombinePath(baseDirectory ?? basePath, interviewId.FormatGuid());
        }
    }
}
