using System;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Messages
{
    public class GetInterviewsRequest : ICommunicationMessage
    {
        public Guid UserId { get; set; }

        public GetInterviewsRequest(Guid userId)
        {
            UserId = userId;
        }
    }

    public class GetInterviewsResponse : ICommunicationMessage
    {

    }
}
