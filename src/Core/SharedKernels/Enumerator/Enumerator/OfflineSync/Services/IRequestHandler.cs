using System;
using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Services
{
    public class CommunicationProgress
    {
        public TimeSpan Eta { get; set; }
        public long TotalBytes { get; set; }
        public long TransferedBytes { get; set; }
        public string Speed { get; set; }
    }

    public interface IRequestHandler
    {
        Task<byte[]> Handle(byte[] message);
        void RegisterHandler<TMessage, TResponse>(Func<TMessage, Task<TResponse>> handler)
            where TMessage : ICommunicationMessage
            where TResponse: ICommunicationMessage;
    }
}
