using System;
using SQLite;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Repositories;

public class TabletBrokenAudioAuditFileStorage : TabletBrokenFileStorage<BrokenAudioAuditMetadataView, BrokenAudioAuditFileView>, IBrokenAudioAuditFileStorage
{
    public TabletBrokenAudioAuditFileStorage(
        IPlainStorage<BrokenAudioAuditMetadataView> fileMetadataViewStorage,
        IPlainStorage<BrokenAudioAuditFileView> fileViewStorage,
        IEncryptionService encryptionService)
        :base(fileMetadataViewStorage, fileViewStorage, encryptionService)
    {
    }
}

public class BrokenAudioAuditMetadataView : IFileMetadataView, IPlainStorageEntity
{
    [PrimaryKey]
    public string Id { get; set; }
    public Guid InterviewId { get; set; }
    public string FileId { get; set; }
    public string FileName { get; set; }
    public string ContentType { get; set; }
}

public class BrokenAudioAuditFileView : IFileView, IPlainStorageEntity
{
    [PrimaryKey]
    public string Id { get; set; }
    public byte[] File { get; set; }
}
