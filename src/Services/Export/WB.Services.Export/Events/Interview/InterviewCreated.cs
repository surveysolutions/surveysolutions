using System;
using WB.Services.Export.Events.Interview.Base;

namespace WB.Services.Export.Events.Interview
{
    public class InterviewCreated : InterviewActiveEvent
    {
        public Guid QuestionnaireId { get; }
        public long QuestionnaireVersion { get; }

        public int? AssignmentId { get; }
        public bool UsesExpressionStorage { get; }
        public DateTime? CreationTime { get; }
        public bool? IsAudioRecordingEnabled { get; set; }
    }
}
