using System;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Repositories;

public class BrokenAudioFileStorage : AudioFileStorage, IBrokenAudioFileStorage
{
    public BrokenAudioFileStorage(IPlainStorageAccessor<AudioFile> filePlainStorageAccessor) : base(filePlainStorageAccessor)
    {
    }
        
    protected override string GetFileId(Guid interviewId, string fileName) => 
        $"broken#{DateTime.UtcNow:o}#{interviewId.FormatGuid()}#{fileName}";

    public Task<InterviewBinaryDataDescriptor> FirstOrDefaultAsync()
    {
        throw new NotImplementedException();
    }
}
