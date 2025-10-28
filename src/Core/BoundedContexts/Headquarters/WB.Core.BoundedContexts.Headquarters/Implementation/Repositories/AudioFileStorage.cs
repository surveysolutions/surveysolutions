using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Repositories
{
    public class AudioFileStorage : AudioFileStorageBase<AudioFile>, IAudioFileStorage
    {
        public AudioFileStorage(IPlainStorageAccessor<AudioFile> filePlainStorageAccessor) 
            : base(filePlainStorageAccessor)
        {
        }
    }
}
