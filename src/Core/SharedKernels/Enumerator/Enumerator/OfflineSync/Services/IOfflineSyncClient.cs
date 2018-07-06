using System;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Services
{
    public interface IOfflineSyncClient
    {
        Task<GetQuestionnaireListResponse> GetQuestionnaireList(string endpoint, IProgress<CommunicationProgress> progress = null);

        Task<OkResponse> PostInterviewAsync(string endpoint, PostInterviewRequest package,
            IProgress<CommunicationProgress> progress = null);

        Task<OkResponse> PostInterviewImageAsync(string endpoint, PostInterviewImageRequest postInterviewImageRequest,
            IProgress<CommunicationProgress> progress = null);

        Task<OkResponse> PostInterviewAudioAsync(string endpoint, PostInterviewAudioRequest postInterviewAudioRequest,
            IProgress<CommunicationProgress> progress = null);
    }
}
