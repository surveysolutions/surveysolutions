using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public class InterviewQuestion
    {
        public InterviewQuestion()
        {
            this.QuestionState = QuestionState.Valid | QuestionState.Enabled;
        }
        public InterviewQuestion(Guid id):this()
        {
            this.Id = id;
        }

        public Guid Id { get; set; }
        public object Answer { get;  set; }
        public List<InterviewQuestionComment> Comments { get; set; }
        public QuestionState QuestionState { get; set; }

        public bool IsValid()
        {
            return QuestionState.HasFlag(QuestionState.Valid);
        }

        public bool IsEnabled()
        {
            return QuestionState.HasFlag(QuestionState.Enabled);
        }

        public bool GetIsFlagged()
        {
            return QuestionState.HasFlag(QuestionState.Flagged);
        }

        public bool GetIsAnswered()
        {
            return QuestionState.HasFlag(QuestionState.Answered);
        }
    }

    [Flags]
    public enum QuestionState
    {
        Valid = 0,
        Enabled = 1,
        Flagged = 2,
        Answered = 4
    }
}
