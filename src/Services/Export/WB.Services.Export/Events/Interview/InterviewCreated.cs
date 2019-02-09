using System;
using WB.Services.Export.Events.Interview.Base;

namespace WB.Services.Export.Events.Interview
{
    public class InterviewCreated : InterviewActiveEvent
    {
        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }

        public int? AssignmentId { get; set; }
        public bool UsesExpressionStorage { get; set; }
        public DateTime? CreationTime { get; set; }
        public bool? IsAudioRecordingEnabled { get; set; }

        public string QuestionnaireIdentity => QuestionnaireId.ToString("N") + "$" +
                                               QuestionnaireVersion;
    }
}
