using System;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Messages
{
    public class GetInterviewDetailsAfterEventRequest : ICommunicationMessage
    {
        public Guid InterviewId { get; set; }
        public Guid EventId { get; set; }

        public GetInterviewDetailsAfterEventRequest(Guid interviewId, Guid eventId)
        {
            InterviewId = interviewId;
            EventId = eventId;
        }
    }
}
