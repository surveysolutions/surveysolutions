using System;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Messages
{
    public class LogInterviewAsSuccessfullyHandledRequest : ICommunicationMessage
    {
        public Guid InterviewId { get; set; }

        public LogInterviewAsSuccessfullyHandledRequest(Guid interviewId)
        {
            InterviewId = interviewId;
        }
    }
}
