using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SQLite;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Repositories;

public class TabletBrokenImageFileStorage : TabletBrokenFileStorage<BrokenImageMetadataView, BrokenImageFileView>, IBrokenImageFileStorage
{
    public TabletBrokenImageFileStorage(
        IPlainStorage<BrokenImageMetadataView> imageViewStorage,
        IPlainStorage<BrokenImageFileView> fileViewStorage,
        IEncryptionService encryptionService)
        : base(imageViewStorage, fileViewStorage, encryptionService)
    {
    }
}

public class BrokenImageMetadataView : IFileMetadataView, IPlainStorageEntity
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

