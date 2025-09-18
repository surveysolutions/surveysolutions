using System;
using System.Threading.Tasks;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Repositories;

public class BrokenAudioAuditFileS3Storage : AudioAuditFileS3Storage, IBrokenAudioAuditFileStorage
{
    protected override string AudioAuditS3Folder => $"{base.AudioAuditS3Folder}broken/";

    public BrokenAudioAuditFileS3Storage(IExternalFileStorage externalFileStorage, 
        IPlainStorageAccessor<AudioAuditFile> filePlainStorageAccessor, 
        IUnitOfWork unitOfWork) : base(externalFileStorage, filePlainStorageAccessor, unitOfWork)
    {
    }

    public Task<InterviewBinaryDataDescriptor> FirstOrDefaultAsync()
    {
        throw new NotImplementedException();
    }
}
