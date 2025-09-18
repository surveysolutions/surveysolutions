using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SQLite;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Repositories;

public class InterviewerBrokenImageFileStorage : InterviewerBrokenFileStorage<BrokenMultimediaView, BrokenImageFileView>, IBrokenImageFileStorage
{
    public InterviewerBrokenImageFileStorage(
        IPlainStorage<BrokenMultimediaView> imageViewStorage,
        IPlainStorage<BrokenImageFileView> fileViewStorage,
        IEncryptionService encryptionService)
        : base(imageViewStorage, fileViewStorage, encryptionService)
    {
    }
}

public class BrokenMultimediaView : IFileMetadataView, IPlainStorageEntity
{
    [PrimaryKey]
    public string Id { get; set; }
    public Guid InterviewId { get; set; }
    public string FileId { get; set; }
    public string FileName { get; set; }
    public string ContentType { get; set; }
}

public class BrokenImageFileView : IFileView, IPlainStorageEntity
{
    [PrimaryKey]
    public string Id { get; set; }
    public byte[] File { get; set; }
}

