﻿using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;

namespace WB.Core.BoundedContexts.Tester.Implementation.Services
{
    public class TesterFileSystemInterviewFileStorage : IFileSystemInterviewFileStorage, IPlainFileCleaner
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly string basePath;
        private const string DataDirectoryName = "InterviewData";

        public TesterFileSystemInterviewFileStorage(IFileSystemAccessor fileSystemAccessor, string rootDirectoryPath)
        {
            this.fileSystemAccessor = fileSystemAccessor;

            this.basePath = this.fileSystemAccessor.CombinePath(rootDirectoryPath, DataDirectoryName);

            if (!this.fileSystemAccessor.IsDirectoryExists(this.basePath))
                this.fileSystemAccessor.CreateDirectory(this.basePath);
        }

        public byte[] GetInterviewBinaryData(Guid interviewId, string fileName)
        {
            var filePath = this.GetPathToFile(fileName);
            if (!this.fileSystemAccessor.IsFileExists(filePath))
                return null;
            return this.fileSystemAccessor.ReadAllBytes(filePath);
        }

        public List<InterviewBinaryDataDescriptor> GetBinaryFilesForInterview(Guid interviewId)
        {
            var directoryPath = this.GetPathToInterviewDirectory();

            if (!this.fileSystemAccessor.IsDirectoryExists(directoryPath))
                return new List<InterviewBinaryDataDescriptor>();

            return this.fileSystemAccessor.GetFilesInDirectory(directoryPath).Select(fileName =>
                new InterviewBinaryDataDescriptor(interviewId, this.fileSystemAccessor.GetFileName(fileName),
                    () => this.fileSystemAccessor.ReadAllBytes(fileName))).ToList();
        }

        public void StoreInterviewBinaryData(Guid interviewId, string fileName, byte[] data)
        {
            var directoryPath = this.GetPathToInterviewDirectory();

            if (!this.fileSystemAccessor.IsDirectoryExists(directoryPath))
                this.fileSystemAccessor.CreateDirectory(directoryPath);

            this.fileSystemAccessor.WriteAllBytes(this.GetPathToFile(fileName), data);
        }

        public void RemoveInterviewBinaryData(Guid interviewId, string fileName)
        {
            var filePath = this.GetPathToFile(fileName);
            if (!this.fileSystemAccessor.IsFileExists(filePath))
                return;

            this.fileSystemAccessor.DeleteFile(filePath);
        }

        private string GetPathToFile(string fileName)
        {
            return this.fileSystemAccessor.CombinePath(this.GetPathToInterviewDirectory(), fileName);
        }

        private string GetPathToInterviewDirectory()
        {
            return this.fileSystemAccessor.CombinePath(this.basePath, "TempInterviewData");
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