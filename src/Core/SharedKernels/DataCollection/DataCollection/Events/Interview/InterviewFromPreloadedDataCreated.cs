using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewFromPreloadedDataCreated : InterviewActiveEvent
    {
        public InterviewFromPreloadedDataCreated(Guid userId, Guid questionnaireId, 
            long questionnaireVersion, int? assignmentId, DateTimeOffset originDate,
            bool usesExpressionStorage = false)
            : base(userId, originDate)
        {
            this.QuestionnaireId = questionnaireId;
            this.QuestionnaireVersion = questionnaireVersion;
            this.UsesExpressionStorage = usesExpressionStorage;
            this.AssignmentId = assignmentId;
        }

        public Guid QuestionnaireId { get; private set; }
        public long QuestionnaireVersion { get; private set; }
        public bool UsesExpressionStorage { get; set; }
        public int? AssignmentId { get; private set; }
    }
}
