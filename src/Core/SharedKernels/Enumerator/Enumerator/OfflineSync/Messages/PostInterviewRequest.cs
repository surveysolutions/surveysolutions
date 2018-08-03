using System;
using System.Collections.Generic;
using Ncqrs.Eventing;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Messages
{
    public class PostInterviewRequest : ICommunicationMessage
    {
        public IReadOnlyCollection<CommittedEvent> Events { get; set; }
        public Guid InterviewId { get; set; }

        public PostInterviewRequest(Guid interviewId, IReadOnlyCollection<CommittedEvent> events)
        {
            this.InterviewId = interviewId;
            this.Events = events;
        }
    }
}
