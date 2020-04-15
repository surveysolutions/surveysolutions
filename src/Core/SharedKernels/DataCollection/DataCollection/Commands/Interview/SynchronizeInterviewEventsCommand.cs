using System;
using Main.Core.Events;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class SynchronizeInterviewEventsCommand : InterviewCommand
    {
        public Guid QuestionnaireId { get; set; }

        public long QuestionnaireVersion { get; set; }

        public AggregateRootEvent[] SynchronizedEvents { get; set; }

        public InterviewStatus InterviewStatus { get;  set; }

        public bool CreatedOnClient { get; set; }

        public InterviewKey InterviewKey { get; }

        public Guid? NewSupervisorId { get; }

        public SynchronizeInterviewEventsCommand(Guid interviewId,
            Guid userId,
            Guid questionnaireId,
            long questionnaireVersion,
            AggregateRootEvent[] synchronizedEvents,
            InterviewStatus interviewStatus,
            bool createdOnClient,
            InterviewKey interviewKey,
            Guid? newSupervisorId)
            : base(interviewId, userId)
        {
            QuestionnaireId = questionnaireId;
            QuestionnaireVersion = questionnaireVersion;
            SynchronizedEvents = synchronizedEvents;
            InterviewStatus = interviewStatus;
            CreatedOnClient = createdOnClient;
            this.InterviewKey = interviewKey;
            NewSupervisorId = newSupervisorId;
        }
    }
}
