using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    public class AssignmentRow
    {
        public DateTime CreatedAtUtc { get; set; }
        public Guid ResponsibleId { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public int? Quantity { get; set; }
        public int InterviewsCount { get; set; }
        public int Id { get; set; }
        public string Responsible { get; set; }
        public string ResponsibleRole { get; set; }
        public QuestionnaireIdentity QuestionnaireId { get; set; }
        public bool Archived { get; set; }
        public string QuestionnaireTitle { get; set; }
        public bool IsAudioRecordingEnabled { get; set; }

        public List<AssignmentIdentifyingQuestionRow> IdentifyingQuestions { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public bool? WebMode { get; set; }

        public DateTime? ReceivedByTabletAtUtc { get; set; }
        public string Comments { get; set; }
        
        public bool WebModeEnabledOnQuestionnaire { get; set; }

        public CalendarEventView CalendarEvent { get; set; }
    }
}
