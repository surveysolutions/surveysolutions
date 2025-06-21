using System;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.InterviewDataStorage
{
    public class InterviewReference
    {
        public string QuestionnaireId { get; set; } = string.Empty;

        public Guid InterviewId { get; set; }

        public InterviewStatus Status { get; set; }

        public string Key { get; set; } = string.Empty;

        public DateTime? UpdateDateUtc { get; set; }

        public DateTime? DeletedAtUtc { get; set; }

        public int? AssignmentId { get; set; }
    }

    public class GeneratedQuestionnaireReference
    {
        protected GeneratedQuestionnaireReference()
        {
        }

        public GeneratedQuestionnaireReference(QuestionnaireId id)
        {
            this.Id = id.ToString();
        }

        public string Id { get; set; } = String.Empty;
        public DateTime? DeletedAt { get; set; }

        public GeneratedQuestionnaireReference MarkAsDeleted()
        {
            this.DeletedAt = DateTime.UtcNow;
            return this;
        }
    }
}
