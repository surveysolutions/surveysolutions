using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Repositories
{
    public class AudioAuditFileS3Storage : AudioAuditFileS3StorageBase<AudioAuditFile>
    {
        protected override string AudioAuditS3Folder => "audio_audit/";

        public AudioAuditFileS3Storage(IExternalFileStorage externalFileStorage, 
            IPlainStorageAccessor<AudioAuditFile> filePlainStorageAccessor, 
            IUnitOfWork unitOfWork) 
            : base(externalFileStorage, filePlainStorageAccessor, unitOfWork)
        {
        }
    }
}
