using System;

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
}
