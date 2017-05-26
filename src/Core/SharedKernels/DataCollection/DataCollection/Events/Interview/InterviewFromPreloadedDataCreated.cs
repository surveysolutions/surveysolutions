using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewFromPreloadedDataCreated : InterviewActiveEvent
    {
        public InterviewFromPreloadedDataCreated(Guid userId, Guid questionnaireId, long questionnaireVersion, bool usesExpressionStorage = false)
            : base(userId)
        {
            this.QuestionnaireId = questionnaireId;
            this.QuestionnaireVersion = questionnaireVersion;
            this.UsesExpressionStorage = usesExpressionStorage;
        }

        public Guid QuestionnaireId { get; private set; }
        public long QuestionnaireVersion { get; private set; }
        public bool UsesExpressionStorage { get; set; }
    }
}
