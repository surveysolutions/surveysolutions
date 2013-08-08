using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewSynchronized : InterviewActiveEvent
    {
        public InterviewSynchronized(Guid userId, Guid questionnaireId, Guid statusId, long questionnaireVersion, Dictionary<Guid, AnswerSynchronizationDto> answers) : base(userId)
        {
            QuestionnaireId = questionnaireId;
            StatusId = statusId;
            QuestionnaireVersion = questionnaireVersion;
            Answers = answers;
        }

        public Guid QuestionnaireId { get; private set; }
        public Guid StatusId { get; private set; }
        public long QuestionnaireVersion { get; private set; }
        public Dictionary<Guid, AnswerSynchronizationDto> Answers { get; private set; }
    }
}
