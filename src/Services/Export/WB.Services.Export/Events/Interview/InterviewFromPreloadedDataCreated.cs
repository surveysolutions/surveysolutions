using System;
using WB.Services.Export.Events.Interview.Base;

namespace WB.Services.Export.Events.Interview
{
    public class InterviewFromPreloadedDataCreated : InterviewActiveEvent
    {
        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }
        public bool UsesExpressionStorage { get; set; }
        public int? AssignmentId { get; set; }
    }
}
