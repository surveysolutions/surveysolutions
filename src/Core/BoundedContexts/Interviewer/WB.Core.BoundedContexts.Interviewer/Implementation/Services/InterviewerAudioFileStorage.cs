using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;


namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
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