using System;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Repositories;

public class BrokenAudioAuditFileStorage : AudioAuditFileStorage, IBrokenAudioAuditFileStorage
{
    public BrokenAudioAuditFileStorage(IPlainStorageAccessor<AudioAuditFile> filePlainStorageAccessor, 
        IUnitOfWork unitOfWork) 
        : base(filePlainStorageAccessor, unitOfWork)
    {
    }

    protected override string GetFileId(Guid interviewId, string fileName) =>
        $"broken#{DateTime.UtcNow:o}#{interviewId.FormatGuid()}#{fileName}";

    public Task<InterviewBinaryDataDescriptor> FirstOrDefaultAsync()
    {
        throw new NotImplementedException();
    }
}
