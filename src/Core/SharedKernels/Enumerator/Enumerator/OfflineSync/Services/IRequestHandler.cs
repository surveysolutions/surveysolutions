using System;
using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Services
{
    public interface IRequestHandler
    {
        Task<ICommunicationMessage> Handle(ICommunicationMessage message);

        void RegisterHandler<TMessage, TResponse>(Func<TMessage, Task<TResponse>> handler)
            where TMessage : ICommunicationMessage
            where TResponse: ICommunicationMessage;
    }
}
