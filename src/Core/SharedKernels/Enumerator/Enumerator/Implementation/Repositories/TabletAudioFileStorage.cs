using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Repositories
{
    public class TabletAudioFileStorage : InterviewFileStorage<AudioFileMetadataView, AudioFileView>, IAudioFileStorage
    {
        public TabletAudioFileStorage(
            IPlainStorage<AudioFileMetadataView> fileMetadataViewStorage,
            IPlainStorage<AudioFileView> fileViewStorage,
            IEncryptionService encryptionService)
            :base(fileMetadataViewStorage, fileViewStorage, encryptionService)
        {
        }
    }
}
