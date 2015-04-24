using System;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public class QuestionOptions
    {
        public virtual int Id { get; set; }
        public virtual InterviewSummary InterviewSummary { get; set; }
        public virtual Guid QuestionId { get; set; }
        public virtual decimal Value { get; set; }
        public virtual string Text { get; set; }
    }
}