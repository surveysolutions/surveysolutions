using System;
using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;

namespace WB.Core.BoundedContexts.Supervisor.Services.Implementation
{
    [Obsolete]
    public class DebugHandler : IHandleCommunicationMessage
    {
        public void Register(IRequestHandler requestHandler)
        {
            requestHandler.RegisterHandler<SendBigAmountOfDataRequest, SendBigAmountOfDataResponse>(Handle);
        }

        public Task<SendBigAmountOfDataResponse> Handle(SendBigAmountOfDataRequest request)
        {
            return Task.FromResult(new SendBigAmountOfDataResponse
            {
                Data = request.Data
            });
        }
    }
}