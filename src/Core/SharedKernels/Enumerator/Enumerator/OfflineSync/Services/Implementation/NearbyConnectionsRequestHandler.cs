using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Services.Implementation
{
    public class NearbyConnectionsRequestHandler : IRequestHandler
    {
        private readonly Dictionary<Type, Func<ICommunicationMessage, Task<ICommunicationMessage>>> handlers
            = new Dictionary<Type, Func<ICommunicationMessage, Task<ICommunicationMessage>>>();
        
        public Task<ICommunicationMessage> Handle(ICommunicationMessage message)
        {
            // ReSharper disable once InconsistentlySynchronizedField - handlers initialization is only occure once during app start
            if (handlers.TryGetValue(message.GetType(), out var handler))
            {
                return handler(message);
            }

            throw new ArgumentException(@"Cannot handle message of type: " + message.GetType().FullName,
                nameof(message));
        }

        public void RegisterHandler<TReq, TResp>(Func<TReq, Task<TResp>> handler)
            where TReq : ICommunicationMessage
            where TResp : ICommunicationMessage
        {
            lock (handlers)
            {
                handlers[typeof(TReq)] = async r =>
                {
                    var result = await handler((TReq)r);
                    return result;
                };
            }
        }
    }
}
