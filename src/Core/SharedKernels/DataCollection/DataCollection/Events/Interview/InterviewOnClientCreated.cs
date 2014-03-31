using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewOnClientCreated : InterviewActiveEvent
    {
        public Guid QuestionnaireId { get; private set; }
        public long QuestionnaireVersion { get; private set; }

        public InterviewOnClientCreated(Guid userId, Guid questionnaireId, long questionnaireVersion)
            : base(userId)
        {
            this.QuestionnaireId = questionnaireId;
            this.QuestionnaireVersion = questionnaireVersion;
        }
    }
}
