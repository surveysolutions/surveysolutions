using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Services
{
    /// <summary>
    /// Used by interviewer only to communicate with connected supervisors
    /// Connection endpoint is defined outside of client
    /// </summary>
    public interface IOfflineSyncClient
    {
        Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken, IProgress<TransferProgress> progress = null)
            where TRequest : ICommunicationMessage
            where TResponse : ICommunicationMessage;
        Task SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken, IProgress<TransferProgress> progress = null) 
            where TRequest : ICommunicationMessage;
    }
}
