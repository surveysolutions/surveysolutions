using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.WebApi;

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
        public List<InterviewApiView> Interviews { get; set; }
    }
}
