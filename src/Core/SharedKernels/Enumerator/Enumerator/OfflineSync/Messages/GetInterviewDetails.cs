using System;
using System.Collections.Generic;
using Ncqrs.Eventing;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Messages
{
    public class GetInterviewDetailsRequest : ICommunicationMessage
    {
        public Guid InterviewId { get; set; }

        public GetInterviewDetailsRequest(Guid interviewId)
        {
            InterviewId = interviewId;
        }
    }

    public class GetInterviewDetailsResponse : ICommunicationMessage
    {
        public List<CommittedEvent> Events { get; set; }
    }
}
