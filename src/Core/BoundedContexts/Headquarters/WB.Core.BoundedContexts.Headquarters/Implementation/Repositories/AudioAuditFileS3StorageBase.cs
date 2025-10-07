using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Repositories;

public abstract class AudioAuditFileS3StorageBase<T> : AudioAuditStorageBase
    where T: AudioAuditFile, new()
{
    protected abstract string AudioAuditS3Folder { get; }
    private readonly IExternalFileStorage externalFileStorage;
    private readonly IPlainStorageAccessor<T> filePlainStorageAccessor;

    protected AudioAuditFileS3StorageBase(
        IExternalFileStorage externalFileStorage,
        IPlainStorageAccessor<T> filePlainStorageAccessor)
    {
        this.externalFileStorage = externalFileStorage ?? throw new ArgumentNullException(nameof(externalFileStorage));
        this.filePlainStorageAccessor = filePlainStorageAccessor ?? throw new ArgumentNullException(nameof(filePlainStorageAccessor));
    }

    public override async Task<byte[]> GetInterviewBinaryDataAsync(Guid interviewId, string fileName)
    {
        string fileId = GetFileId(interviewId, fileName);
        var audioAuditData = filePlainStorageAccessor.GetById(fileId);
        if (audioAuditData.Data == null)
        {
            return await externalFileStorage.GetBinaryAsync(AudioAuditS3Folder + fileId);
        }
        return audioAuditData.Data;
    }

    public override byte[] GetInterviewBinaryData(Guid interviewId, string fileName)
    {
        throw new NotImplementedException();
    }

    public override Task<List<InterviewBinaryDataDescriptor>> GetBinaryFilesForInterview(Guid interviewId)
    {
        var databaseFiles = filePlainStorageAccessor.Query(q => q.Where(f => f.InterviewId == interviewId));

        var interviewBinaryDataDescriptors = databaseFiles.Select(file
            => new InterviewBinaryDataDescriptor(
                interviewId,
                file.FileName,
                file.ContentType,
                () => GetInterviewBinaryDataAsync(interviewId, file.FileName)
            )).ToList();
        return Task.FromResult(interviewBinaryDataDescriptors);
    }

    public override void StoreInterviewBinaryData(Guid interviewId, string fileName, byte[] data, string contentType)
    {
        var id = GetFileId(interviewId, fileName);
        var file = new T
        {
            Id = id,
            InterviewId = interviewId,
            FileName = fileName,
            ContentType = contentType,
            Data = null,
        };
        filePlainStorageAccessor.Store(file, file.Id);
        externalFileStorage.Store(AudioAuditS3Folder + id, data, contentType);
    }

    public override async Task RemoveInterviewBinaryData(Guid interviewId, string fileName)
    {
        var fileId = GetFileId(interviewId, fileName);
        filePlainStorageAccessor.Remove(fileId);
        await externalFileStorage.RemoveAsync(AudioAuditS3Folder + fileId);
    }
}
