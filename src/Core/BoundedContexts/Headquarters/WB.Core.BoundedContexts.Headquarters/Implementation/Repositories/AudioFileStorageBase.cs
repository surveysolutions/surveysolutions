using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Repositories;

public abstract class AudioFileStorageBase<T> : IAudioFileStorage 
    where T : AudioFile, new()
{
    private readonly IPlainStorageAccessor<T> filePlainStorageAccessor;

    protected AudioFileStorageBase(IPlainStorageAccessor<T> filePlainStorageAccessor)
    {
        this.filePlainStorageAccessor = filePlainStorageAccessor;
    }
        
    protected virtual string GetFileId(Guid interviewId, string fileName) => $"{interviewId}#{fileName}";
        
    public Task<byte[]> GetInterviewBinaryDataAsync(Guid interviewId, string fileName) 
        => Task.FromResult(this.GetInterviewBinaryData(interviewId, fileName));

    public byte[] GetInterviewBinaryData(Guid interviewId, string fileName)
    {
        var fileId = GetFileId(interviewId, fileName);
        var databaseFile = filePlainStorageAccessor.GetById(fileId);

        return databaseFile?.Data;
    }

    public Task<List<InterviewBinaryDataDescriptor>> GetBinaryFilesForInterview(Guid interviewId)
    {
        var databaseFiles = filePlainStorageAccessor.Query(q => 
            q.Where(f => f.InterviewId == interviewId)
        );

        var interviewBinaryDataDescriptors = databaseFiles.Select(file 
            => new InterviewBinaryDataDescriptor(
                interviewId, 
                file.FileName,
                file.ContentType,
                () => GetInterviewBinaryDataAsync(interviewId, file.FileName)
            )).ToList();
        return Task.FromResult(interviewBinaryDataDescriptors);
    }

    public void StoreInterviewBinaryData(Guid interviewId, string fileName, byte[] data, string contentType)
    {
        var fileId = GetFileId(interviewId, fileName);
        var file = new T
        {
            Id = fileId,
            InterviewId = interviewId,
            FileName = fileName,
            Data = data,
            ContentType = contentType
        };

        filePlainStorageAccessor.Store(file, fileId);
    }

    public Task RemoveInterviewBinaryData(Guid interviewId, string fileName)
    {
        var fileId = GetFileId(interviewId, fileName);
        filePlainStorageAccessor.Remove(fileId);
        return Task.CompletedTask;
    }
}
