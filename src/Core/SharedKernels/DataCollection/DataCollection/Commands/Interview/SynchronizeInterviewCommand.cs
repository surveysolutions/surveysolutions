using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class SynchronizeInterviewCommand : InterviewCommand
    {
        public SynchronizeInterviewCommand(Guid interviewId, 
            Guid userId,
            AnsweredQuestionSynchronizationDto[] featuredQuestionsMeta,
            bool createdOnClient,
            InterviewStatus initialStatus,
            InterviewSynchronizationDto sycnhronizedInterview) : base(interviewId, userId)
        {
            this.FeaturedQuestionsMeta = featuredQuestionsMeta;
            this.CreatedOnClient = createdOnClient;
            this.InitialStatus = initialStatus;
            SynchronizedInterview = sycnhronizedInterview;
        }

        public AnsweredQuestionSynchronizationDto[] FeaturedQuestionsMeta { get; }
        public bool CreatedOnClient { get; }
        public InterviewStatus InitialStatus { get; }
        public InterviewSynchronizationDto SynchronizedInterview { get; private set; }
    }
}
