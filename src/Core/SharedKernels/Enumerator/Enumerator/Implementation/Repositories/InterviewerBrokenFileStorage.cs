using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Repositories;

public abstract class InterviewerBrokenFileStorage<TMetadataView, TFileView> :
    InterviewerFileStorage<TMetadataView, TFileView>,
    IInterviewBrokenFileStorage
    where TMetadataView : class, IFileMetadataView, IPlainStorageEntity, new()
    where TFileView : class, IFileView, IPlainStorageEntity, new()
{
    protected InterviewerBrokenFileStorage(
        IPlainStorage<TMetadataView> fileMetadataViewStorage,
        IPlainStorage<TFileView> fileViewStorage,
        IEncryptionService encryptionService)
        : base(fileMetadataViewStorage, fileViewStorage, encryptionService)
    {
    }
        

    public Task<InterviewBinaryDataDescriptor> FirstOrDefaultAsync()
    {
        var metadataView = this.fileMetadataViewStorage.FirstOrDefault();
        if (metadataView == null)
            return Task.FromResult<InterviewBinaryDataDescriptor>(null);
            
        var interviewBinaryDataDescriptor = 
            new InterviewBinaryDataDescriptor(
                metadataView.InterviewId,
                metadataView.FileName,
                metadataView.ContentType,
                () => Task.FromResult(this.GetFileById(metadataView.FileId))
            );
        return Task.FromResult(interviewBinaryDataDescriptor);
    }
}
