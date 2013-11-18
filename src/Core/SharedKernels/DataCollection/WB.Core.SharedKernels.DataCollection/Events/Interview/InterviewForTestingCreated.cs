namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    using System;
    using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

    public class InterviewForTestingCreated : InterviewActiveEvent
    {
        public Guid QuestionnaireId { get; private set; }
        public long QuestionnaireVersion { get; private set; }

        public InterviewForTestingCreated(Guid userId, Guid questionnaireId, long questionnaireVersion)
            : base(userId)
        {
            this.QuestionnaireId = questionnaireId;
            this.QuestionnaireVersion = questionnaireVersion;
        }
    }
}
