using System;
using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Services
{
    public interface IOfflineSyncClient
    {
        Task<GetQuestionnaireListResponse> GetQuestionnaireList(string endpoint, IProgress<CommunicationProgress> progress = null);
        Task<SendBigAmountOfDataResponse> SendBigData(string endpoint, byte[] data, IProgress<CommunicationProgress> progress = null);
    }
}
