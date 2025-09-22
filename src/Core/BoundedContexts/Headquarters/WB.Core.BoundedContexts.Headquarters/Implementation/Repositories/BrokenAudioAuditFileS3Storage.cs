using System;
using System.Threading.Tasks;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Repositories;

public class BrokenAudioAuditFileS3Storage : AudioAuditFileS3StorageBase<BrokenAudioAuditFile>, IBrokenAudioAuditFileStorage
{
    protected override string AudioAuditS3Folder => $"broken/audio_audit/";

    public BrokenAudioAuditFileS3Storage(IExternalFileStorage externalFileStorage, 
        IPlainStorageAccessor<BrokenAudioAuditFile> filePlainStorageAccessor) 
        : base(externalFileStorage, filePlainStorageAccessor)
    {
    }

    public Task<InterviewBinaryDataDescriptor> FirstOrDefaultAsync()
    {
        throw new NotImplementedException();
    }

    public override Task<bool> HasAnyAudioAuditFilesStoredAsync(QuestionnaireIdentity questionnaire)
    {
        throw new NotImplementedException();
    }
}
