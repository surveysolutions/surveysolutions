using System;
using WB.Services.Export.Events.Interview.Base;

namespace WB.Services.Export.Events.Interview
{
    public class InterviewOnClientCreated : InterviewActiveEvent
    {
        public Guid QuestionnaireId { get; }
        public long QuestionnaireVersion { get; }

        public int? AssignmentId { get; }
        public bool UsesExpressionStorage { get; set; }

        public bool? IsAudioRecordingEnabled { get; set; }
    }
}
