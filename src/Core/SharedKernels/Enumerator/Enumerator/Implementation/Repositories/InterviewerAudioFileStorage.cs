using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Repositories
{
    public class InterviewerAudioFileStorage : InterviewerFileStorage<AudioFileMetadataView, AudioFileView>, IAudioFileStorage
    {
        public InterviewerAudioFileStorage(
            IPlainStorage<AudioFileMetadataView> fileMetadataViewStorage,
            IPlainStorage<AudioFileView> fileViewStorage)
            :base(fileMetadataViewStorage, fileViewStorage)
        {
        }
    }
}
