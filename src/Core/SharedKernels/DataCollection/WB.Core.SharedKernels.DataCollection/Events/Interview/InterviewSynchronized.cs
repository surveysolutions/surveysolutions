using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewSynchronized : InterviewActiveEvent
    {
        public InterviewSynchronized(Guid userId, Guid questionnaireId, InterviewStatus status, long questionnaireVersion, Dictionary<Guid, AnswerSynchronizationDto> answers)
            : base(userId)
        {
            QuestionnaireId = questionnaireId;
            Status = status;
            QuestionnaireVersion = questionnaireVersion;
            Answers = answers;
        }

        public Guid QuestionnaireId { get; private set; }
        public InterviewStatus Status { get; private set; }
        public long QuestionnaireVersion { get; private set; }
        public Dictionary<Guid, AnswerSynchronizationDto> Answers { get; private set; }
    }
}
