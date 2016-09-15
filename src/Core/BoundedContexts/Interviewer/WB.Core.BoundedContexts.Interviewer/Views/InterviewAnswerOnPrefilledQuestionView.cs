using System;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    [Obsolete("Remove when all clients are upgrated to 5.13")]
    public class InterviewAnswerOnPrefilledQuestionView
    {
        public Guid QuestionId { get; set; }
        public string QuestionText { get; set; }
        public string Answer { get; set; }
    }
}