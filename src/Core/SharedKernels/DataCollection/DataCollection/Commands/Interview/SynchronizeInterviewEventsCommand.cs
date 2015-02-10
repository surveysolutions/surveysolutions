using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class SynchronizeInterviewEventsCommand : InterviewCommand
    {
        public Guid QuestionnaireId { get; set; }

        public long QuestionnaireVersion { get; set; }

        public object[] SynchronizedEvents { get; set; }

        public InterviewStatus InterviewStatus { get;  set; }

        public bool CreatedOnClient { get; set; }

        public SynchronizeInterviewEventsCommand(Guid interviewId, Guid userId, Guid questionnaireId, long questionnaireVersion, object[] synchronizedEvents, InterviewStatus interviewStatus, bool createdOnClient)
            : base(interviewId, userId)
        {
            QuestionnaireId = questionnaireId;
            QuestionnaireVersion = questionnaireVersion;
            SynchronizedEvents = synchronizedEvents;
            InterviewStatus = interviewStatus;
            CreatedOnClient = createdOnClient;
        }
    }
}
