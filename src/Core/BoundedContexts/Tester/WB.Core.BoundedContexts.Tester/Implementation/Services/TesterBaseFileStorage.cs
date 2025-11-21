using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;

namespace WB.Core.BoundedContexts.Tester.Implementation.Services
{
    public abstract class TesterBaseFileStorage : IInterviewFileStorage, IPlainFileCleaner
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly string basePath;

        protected abstract string DataDirectoryName { get; }
        protected abstract string EntityDirectoryName { get; }

        protected TesterBaseFileStorage(IFileSystemAccessor fileSystemAccessor, string rootDirectoryPath)
        {
            this.fileSystemAccessor = fileSystemAccessor;

            this.basePath = this.fileSystemAccessor.CombinePath(rootDirectoryPath, DataDirectoryName);

            if (!this.fileSystemAccessor.IsDirectoryExists(this.basePath))
                this.fileSystemAccessor.CreateDirectory(this.basePath);
        }

        public Task<byte[]> GetInterviewBinaryDataAsync(Guid interviewId, string fileName) 
            => Task.FromResult(this.GetInterviewBinaryData(interviewId, fileName));

        public byte[] GetInterviewBinaryData(Guid interviewId, string fileName)
        {
            var fileNameWithoutPath = fileSystemAccessor.GetFileName(fileName);
            var filePath = this.GetPathToFile(fileNameWithoutPath);

            return !fileSystemAccessor.IsFileExists(filePath) ? null : fileSystemAccessor.ReadAllBytes(filePath);
        }

        public Task<List<InterviewBinaryDataDescriptor>> GetBinaryFilesForInterview(Guid interviewId)
        {
            var directoryPath = this.GetPathToInterviewDirectory();

            if (!this.fileSystemAccessor.IsDirectoryExists(directoryPath))
                return Task.FromResult(new List<InterviewBinaryDataDescriptor>());

            var interviewBinaryDataDescriptors = this.fileSystemAccessor.GetFilesInDirectory(directoryPath).Select(filePath =>
                new InterviewBinaryDataDescriptor(
                    interviewId, 
                    this.fileSystemAccessor.GetFileName(filePath),
                    null,
                    () => Task.FromResult(this.fileSystemAccessor.ReadAllBytes(filePath)),
                    null))
                .ToList();
            return Task.FromResult(interviewBinaryDataDescriptors);
        }

        public void StoreInterviewBinaryData(Guid interviewId, string fileName, byte[] data, string contentType)
        {
            var directoryPath = this.GetPathToInterviewDirectory();

            if (!this.fileSystemAccessor.IsDirectoryExists(directoryPath))
                this.fileSystemAccessor.CreateDirectory(directoryPath);

            var filePath = this.GetPathToFile(fileName);

            if (fileSystemAccessor.IsFileExists(filePath))
                fileSystemAccessor.DeleteFile(filePath);

            fileSystemAccessor.WriteAllBytes(filePath, data);
        }

        public Task RemoveInterviewBinaryData(Guid interviewId, string fileName)
        {
            var filePath = this.GetPathToFile(fileName);
            if (this.fileSystemAccessor.IsFileExists(filePath))
                this.fileSystemAccessor.DeleteFile(filePath);
            return Task.CompletedTask;
        }

        private string GetPathToFile(string fileName)
        {
            if (fileSystemAccessor.IsInvalidFileName(fileName))
                throw new ArgumentException("Invalid file name", nameof(fileName));
            return this.fileSystemAccessor.CombinePath(this.GetPathToInterviewDirectory(), fileName);
        }

        private string GetPathToInterviewDirectory()
        {
            return this.fileSystemAccessor.CombinePath(this.basePath, EntityDirectoryName);
        }

        public void Clear()
        {
            var directoryPath = this.GetPathToInterviewDirectory();

            if (!this.fileSystemAccessor.IsDirectoryExists(directoryPath))
                return;

            var files = this.fileSystemAccessor.GetFilesInDirectory(directoryPath);
            var directories = this.fileSystemAccessor.GetDirectoriesInDirectory(directoryPath);

            foreach (var file in files)
                this.fileSystemAccessor.DeleteFile(file);

            foreach (var directory in directories)
                this.fileSystemAccessor.DeleteDirectory(directory);
        }
    }
}
