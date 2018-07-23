using System;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Messages
{
    public class SupervisorIdRequest : ICommunicationMessage
    {
    }

    public class SupervisorIdResponse : ICommunicationMessage
    {
        public Guid SupervisorId { get; set; }
    }
}
