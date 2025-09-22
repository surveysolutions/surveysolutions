using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Repositories
{
    public class AudioAuditFileStorage : AudioAuditFileStorageBase<AudioAuditFile> 
    {
        public AudioAuditFileStorage(IPlainStorageAccessor<AudioAuditFile> filePlainStorageAccessor, IUnitOfWork unitOfWork) 
            : base(filePlainStorageAccessor, unitOfWork)
        {
        }
    }
}
