using System;
using WB.Services.Export.Events.Interview.Base;

namespace WB.Services.Export.Events.Interview
{
    public class InterviewFromPreloadedDataCreated : InterviewActiveEvent
    {
        public Guid QuestionnaireId { get; private set; }
        public long QuestionnaireVersion { get; private set; }
        public bool UsesExpressionStorage { get; set; }
        public int? AssignmentId { get; private set; }
    }
}
