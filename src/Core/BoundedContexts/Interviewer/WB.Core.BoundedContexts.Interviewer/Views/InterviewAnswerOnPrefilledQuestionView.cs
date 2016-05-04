using System;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class InterviewAnswerOnPrefilledQuestionView
    {
        public Guid QuestionId { get; set; }
        public string QuestionText { get; set; }
        public string Answer { get; set; }
    }
}