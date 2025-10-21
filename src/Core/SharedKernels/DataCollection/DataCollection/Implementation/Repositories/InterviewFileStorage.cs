using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Configs;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Repositories;

public abstract class InterviewFileStorage: IInterviewFileStorage
{
    protected readonly IFileSystemAccessor fileSystemAccessor;
    private readonly string basePath;

    public InterviewFileStorage(IFileSystemAccessor fileSystemAccessor, IOptions<FileStorageConfig> rootDirectoryPath)
    {
        this.fileSystemAccessor = fileSystemAccessor;
        
        this.basePath = rootDirectoryPath.Value.AppData;

        if (!this.fileSystemAccessor.IsDirectoryExists(this.basePath))
            this.fileSystemAccessor.CreateDirectory(this.basePath);
    }

    public Task<byte[]> GetInterviewBinaryDataAsync(Guid interviewId, string fileName) 
        => Task.FromResult(this.GetInterviewBinaryData(interviewId, fileName));

    public byte[] GetInterviewBinaryData(Guid interviewId, string fileName)
    {
        var fileNameWithoutPath = fileSystemAccessor.GetFileName(fileName);
        var filePath = this.GetPathToFile(interviewId, fileNameWithoutPath);

        return !fileSystemAccessor.IsFileExists(filePath) ? null : fileSystemAccessor.ReadAllBytes(filePath);
    }

    public Task<List<InterviewBinaryDataDescriptor>> GetBinaryFilesForInterview(Guid interviewId)
    {
        var directoryPath = this.GetPathToInterviewDirectory(interviewId, basePath);
            
        if (!fileSystemAccessor.IsDirectoryExists(directoryPath))
            return Task.FromResult(new List<InterviewBinaryDataDescriptor>());

        var interviewBinaryDataDescriptors = fileSystemAccessor.GetFilesInDirectory(directoryPath)
            .Select(
                fileName =>
                    new InterviewBinaryDataDescriptor(
                        interviewId, 
                        fileSystemAccessor.GetFileName(fileName),
                        ContentType,
                        () => Task.FromResult(fileSystemAccessor.ReadAllBytes(fileName)))).ToList();
        return Task.FromResult(interviewBinaryDataDescriptors);
    }

    public void StoreInterviewBinaryData(Guid interviewId, string fileName, byte[] data, string contentType)
    {
        var directoryPath = this.GetPathToInterviewDirectory(interviewId, basePath);
            
        if (!fileSystemAccessor.IsDirectoryExists(directoryPath))
            fileSystemAccessor.CreateDirectory(directoryPath);

        var filePath = this.GetPathToFile(interviewId, fileName);

        if (fileSystemAccessor.IsFileExists(filePath))
            fileSystemAccessor.DeleteFile(filePath);

        fileSystemAccessor.WriteAllBytes(filePath, data);
    }
        
    public Task RemoveInterviewBinaryData(Guid interviewId, string fileName)
    {
        var filePath = this.GetPathToFile(interviewId, fileName);
        if (!fileSystemAccessor.IsFileExists(filePath))
            return Task.CompletedTask;

        fileSystemAccessor.DeleteFile(filePath);
        return Task.CompletedTask;
    }

    public string GetPath(Guid interviewId, string filename = null) => this.GetPathToFile(interviewId, filename);

    public Task RemoveAllBinaryDataForInterviewsAsync(List<Guid> interviewIds)
    {
        Parallel.ForEach(interviewIds,
            interviewId =>
            {
                var directoryPath = this.GetPathToInterviewDirectory(interviewId, basePath);
                if (fileSystemAccessor.IsDirectoryExists(directoryPath))
                    fileSystemAccessor.DeleteDirectory(directoryPath);
            });
           
        return Task.CompletedTask;
    }

    private string GetPathToFile(Guid interviewId, string fileName)
    {
        return fileSystemAccessor.CombinePath(GetPathToInterviewDirectory(interviewId, basePath), fileName);
    }

    protected abstract string GetPathToInterviewDirectory(Guid interviewId, string baseDirectory);
    protected abstract string ContentType { get; }
}
